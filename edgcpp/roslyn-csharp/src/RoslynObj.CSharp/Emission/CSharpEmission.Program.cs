using System.Text.Json;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private ProgramAssembly? selectedProgram;
        private int programOpenUnitLimit;

        /// <summary>Opt-in bounded selection; it never searches by a source/display name.</summary>
        private bool InitializeProgramAssembly(ProgramAssemblyOptions assemblyOptions)
        {
            if (selectedProgram is not null) throw new InvalidOperationException("Program selection was already initialized.");
            var result = new ProgramAssembler().Assemble(inputs, assemblyOptions);
            diagnostics.AddRange(result.Diagnostics);
            if (!result.Success) return false;
            selectedProgram = result.Program!;
            programOpenUnitLimit = assemblyOptions.MaximumOpenUnits;
            foreach (var declaration in selectedProgram.Report.Declarations.Where(declaration => declaration.Required && declaration.SelectedDefinition is null))
                diagnostics.Add(new("CSHARP2501", DiagnosticSeverity.Error,
                    "Initial multi-unit execution requires a selected program definition; native provider contract integration remains unsupported.",
                    inputs[declaration.UnitOrdinal - 1].Manifest.ArtifactIdentity, declaration.Locator, "program-provider"));
            foreach (var function in selectedProgram.Functions)
            {
                try
                {
                    var signature = function.Definition.Signature;
                    if (signature.IsVariadic)
                        throw Limit("Multi-unit variadic functions require a separately exercised program ABI contract.", "program-abi");
                    ValidateOrdinaryRoutineRoles(signature);
                    foreach (var type in signature.Parameters.Select(parameter => parameter.Type).Append(signature.ReturnType))
                        ValidateProgramBoundaryType(type);
                }
                catch (Unsupported ex)
                {
                    diagnostics.Add(new("CSHARP2500", DiagnosticSeverity.Error, ex.Message,
                        inputs[function.UnitOrdinal - 1].Manifest.ArtifactIdentity, function.Locator, ex.Family));
                }
            }
            return !diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        }

        private IEnumerable<FunctionSymbol> SelectedFunctions() => selectedProgram is { } program
            ? program.Functions.OrderBy(function => function.SelectedOrdinal).Select(function => function.Definition)
            : inputs.SelectMany(input => input.TranslationUnit.Functions);

        private IEnumerable<VariableSymbol> SelectedStorage()
        {
            if (selectedProgram is { } program)
                return program.Storage.OrderBy(storage => storage.SelectedOrdinal).Select(storage => storage.Definition);
            var localStorage = inputs.SelectMany(input => input.TranslationUnit.Functions).Where(function => function.Body is not null)
                .SelectMany(function => function.Body!.Locals).ToHashSet();
            return inputs.SelectMany(input => input.TranslationUnit.Variables)
                .Where(variable => !variable.Details.IsParameter && !localStorage.Contains(variable));
        }

        private FunctionSymbol CanonicalFunction(FunctionSymbol declaration)
            => selectedProgram?.Resolve(declaration)?.Definition ?? declaration;
        private VariableSymbol CanonicalVariable(VariableSymbol declaration)
            => selectedProgram?.Resolve(declaration)?.Definition ?? declaration;
        private SemanticEntity CanonicalEntity(SemanticEntity entity) => entity switch
        {
            FunctionSymbol function => CanonicalFunction(function),
            VariableSymbol variable => CanonicalVariable(variable),
            _ => entity
        };

        /// <summary>Reads the one selected allocation through this occurrence's recorded TU type.</summary>
        private bool TryProgramReference(ReferenceOperation reference, out string rendered)
        {
            rendered = "";
            if (selectedProgram is null) return false;
            if (reference.Target is FunctionSymbol function)
            {
                rendered = FunctionAddress(CanonicalFunction(function));
                return true;
            }
            if (reference.Target is not VariableSymbol variable || selectedProgram.Resolve(variable) is not { } selected) return false;
            var definition = selected.Definition;
            if (!storage.ContainsKey(definition))
                throw Limit("A bound storage reference has no admitted selected allocation.", "program-storage");
            rendered = $"(*(({TypeName(reference.Type)}*){OwnedName(definition)}))";
            return true;
        }

        /// <summary>An opaque pointee needs no emitted fields; the native data address remains exact.</summary>
        private bool TryProgramPointerType(PointerType pointer, out string representation)
        {
            representation = "";
            if (MemoryRepresentationType(pointer.PointeeType) is not RecordType record || !record.IsIncomplete && record.Layout is not null && record.Size is not null)
                return false;
            ValidateProgramPointer(pointer);
            representation = "void*";
            return true;
        }

        private static void ValidateProgramPointer(PointerType pointer)
        {
            var detail = pointer.Pointer;
            if (pointer.Size != 8 || pointer.Alignment != 8 || detail.IsReference || detail.IsRvalueReference || detail.IsHandle
                || detail.IsTrackingReference || detail.IsInterior || detail.IsPinned || detail.IsPtr32 || detail.IsSptr || detail.IsUptr
                || detail.BasedVariable is not null)
                throw Limit("Multi-unit pointer views require the exercised ordinary 64-bit native address representation.", "program-pointer");
        }

        private static void ValidateProgramBoundaryType(CppType type)
        {
            for (var current = type; current is TypeReference reference; current = reference.UnderlyingType)
                if (current is QualifiedType qualified && (qualified.Qualification.AddressSpace is not (null or 0)
                    || qualified.Qualification.IsShared || qualified.Qualification.IsNear || qualified.Qualification.IsFar
                    || qualified.Qualification.IsUpcStrict || qualified.Qualification.IsUpcRelaxed || qualified.Qualification.IsAtomic))
                    throw Limit("This selected boundary qualifier needs a separate native representation adapter.", "program-abi");
            var raw = Unwrap(type);
            if (raw.Kind.Name == "tk_void") return;
            if (type.Size != raw.Size || type.Alignment != raw.Alignment)
                throw Limit("An effective boundary size/alignment differs from its underlying scalar representation; no adapter is selected.", "program-abi");
            if (raw is PointerType pointer) { ValidateProgramPointer(pointer); return; }
            if (raw is ArithmeticType && raw.Size is { } scalarSize && scalarSize > 0 && scalarSize <= 8) return;
            throw Limit("Initial multi-unit emission supports fixed scalar/pointer function boundaries; aggregate and extended-value adapters remain unsupported.", "program-abi");
        }

        private string ProgramArgumentView(CppType actualType, CppType formalType, string value)
            => ProgramPointerView(actualType, formalType, value);
        private string ProgramResultView(CppType selectedReturnType, CppType occurrenceType, string value)
            => ProgramPointerView(selectedReturnType, occurrenceType, value);
        private bool TryProgramPointerRepresentation(CppType left, CppType right, out bool compatible)
        {
            compatible = false;
            if (selectedProgram is null) return false;
            var leftType = Unwrap(left); var rightType = Unwrap(right);
            if (leftType is not PointerType && rightType is not PointerType) return false;
            if (leftType is not PointerType leftPointer || rightType is not PointerType rightPointer) return true;
            ValidateProgramPointer(leftPointer); ValidateProgramPointer(rightPointer);
            if (left.Size != leftType.Size || left.Alignment != leftType.Alignment
                || right.Size != rightType.Size || right.Alignment != rightType.Alignment)
                throw Limit("Function address dispatch requires undecorated effective pointer size/alignment.", "program-pointer");
            var comparison = new AbiCompatibility().CompareTypes(left, right);
            if (comparison.Status == AbiCompatibilityStatus.Unsupported)
                throw Limit($"Function address dispatch has no selected pointer ABI adapter at {comparison.Path}: {comparison.Reason}", "program-pointer");
            compatible = comparison.Compatible;
            return true;
        }
        private string ProgramPointerView(CppType actualType, CppType expectedType, string value)
        {
            if (selectedProgram is null) return value;
            var actual = Unwrap(actualType); var expected = Unwrap(expectedType);
            if (actual is not PointerType actualPointer || expected is not PointerType expectedPointer) return value;
            if (actualType.Size != actual.Size || actualType.Alignment != actual.Alignment
                || expectedType.Size != expected.Size || expectedType.Alignment != expected.Alignment)
                throw Limit("An effective pointer view has unsupported size/alignment decoration.", "program-pointer");
            ValidateProgramPointer(actualPointer); ValidateProgramPointer(expectedPointer);
            var comparison = new AbiCompatibility().CompareTypes(actualType, expectedType);
            if (!comparison.Compatible)
                throw Limit($"Selected pointer representations have no shared runtime view at {comparison.Path}: {comparison.Reason}", "program-pointer");
            var expectedName = TypeName(expectedType);
            return TypeName(actualType) == expectedName ? value : $"unchecked(({expectedName})({value}))";
        }

        private string? CurrentArtifactIdentity()
        {
            if (currentFunction is not null && selectedProgram?.Resolve(currentFunction) is { } selected)
                return inputs[selected.UnitOrdinal - 1].Manifest.ArtifactIdentity;
            return inputs.FirstOrDefault()?.Manifest.ArtifactIdentity;
        }

        private string ProgramAssemblyReport()
        {
            var program = selectedProgram ?? throw new InvalidOperationException("No program selection exists.");
            return JsonSerializer.Serialize(new
            {
                schemaVersion = 1,
                maximumOpenUnits = programOpenUnitLimit,
                boundedScaleAccepted = false,
                program.Report,
                functions = program.Functions.Select(function => new
                {
                    function.Locator, function.UnitOrdinal, function.SelectedOrdinal, function.ExternalName,
                    generatedName = names.GetValueOrDefault(function.Definition),
                    addressField = functionAddresses.FirstOrDefault(address => address.Function.Equals(function.Definition))?.Field
                }),
                storage = program.Storage.Select(item => new
                {
                    item.Locator, item.UnitOrdinal, item.SelectedOrdinal, item.ExternalName,
                    generatedName = names.GetValueOrDefault(item.Definition),
                    allocationField = storage.GetValueOrDefault(item.Definition)?.OwnerField
                })
            }, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
