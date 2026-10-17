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

        private IEnumerable<FunctionSymbol> SelectedFunctions() => options.ManagedProgramSelection is { } managed
            ? managed.Units.SelectMany(unit => unit.Functions).Select(CanonicalFunction).Distinct()
            : options.ManagedSelection is { } selection
            ? selection.Functions
            : selectedProgram is { } program
            ? program.Functions.OrderBy(function => function.SelectedOrdinal).Select(function => function.Definition)
            : inputs.SelectMany(input => input.TranslationUnit.Functions);

        private IEnumerable<VariableSymbol> SelectedStorage()
        {
            if (options.ManagedProgramSelection is { } managed) return managed.Units.SelectMany(unit => unit.StorageDefinitions).Select(CanonicalVariable).Distinct();
            if (options.ManagedSelection is { } selection) return selection.StorageDefinitions;
            if (selectedProgram is { } program)
                return program.Storage.OrderBy(storage => storage.SelectedOrdinal).Select(storage => storage.Definition);
            var localStorage = inputs.SelectMany(input => input.TranslationUnit.Functions).Where(function => function.Body is not null)
                .SelectMany(function => function.Body!.Locals).ToHashSet();
            return inputs.SelectMany(input => input.TranslationUnit.Variables)
                .Where(variable => !variable.Details.IsParameter && !localStorage.Contains(variable));
        }

        private FunctionSymbol CanonicalFunction(FunctionSymbol declaration)
            => options.ManagedProgramSelection?.FunctionDefinitions.GetValueOrDefault(declaration) ?? selectedProgram?.Resolve(declaration)?.Definition ?? declaration;
        private VariableSymbol CanonicalVariable(VariableSymbol declaration)
            => options.ManagedProgramSelection?.VariableDefinitions.GetValueOrDefault(declaration) ?? selectedProgram?.Resolve(declaration)?.Definition ?? declaration;
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
            if (options.ManagedProgramSelection is { } managed)
            {
                if (reference.Target is FunctionSymbol managedFunction)
                {
                    rendered = FunctionAddress(CanonicalFunction(managedFunction));
                    return true;
                }
                if (reference.Target is not VariableSymbol managedVariable ||
                    !managed.VariableDefinitions.TryGetValue(managedVariable, out var managedDefinition)) return false;
                if (!storage.ContainsKey(managedDefinition))
                    throw Limit("A managed-bound storage reference has no selected allocation.", "program-storage");
                rendered = $"(*(({TypeName(reference.Type)}*){OwnedName(managedDefinition)}))";
                return true;
            }
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

        private void ValidateManagedBoundFunctionAddress(FunctionSymbol function)
        {
            if (options.ManagedProgramSelection is not { } program) return;
            foreach (var binding in program.FunctionBindings.Where(binding => binding.Declaration.Equals(function) || binding.Definition.Equals(function)))
            {
                // The established scalar entry machinery needs no TU-specific
                // pointer view. Pointer-bearing selected boundaries currently
                // have only direct occurrence adapters, not callback adapters.
                foreach (var type in binding.Declaration.Signature.Parameters.Select(parameter => parameter.Type).Append(binding.Declaration.ReturnType)
                    .Concat(binding.Definition.Signature.Parameters.Select(parameter => parameter.Type).Append(binding.Definition.ReturnType)))
                    if (Unwrap(type) is PointerType)
                        throw Limit("A pointer-bearing managed-bound function address requires a separately selected entry/indirect-call view; only direct occurrences are admitted.", "managed-call-address");
            }
        }

        private bool IsManagedBoundCall(CallOperation call, FunctionSymbol definition, out FunctionSymbol declaration)
        {
            declaration = null!;
            if (options.ManagedProgramSelection is not { } program ||
                call.Callee is not ReferenceOperation { Target: FunctionSymbol captured } ||
                !program.FunctionDefinitions.TryGetValue(captured, out var selected)) return false;
            if (TryValidateManagedBoundConstructorCall(call, definition))
            {
                declaration = captured;
                return true;
            }
            if (!selected.Equals(definition) || currentFunction is null || !functions.Contains(currentFunction) ||
                call.TargetMethod is null || !call.TargetMethod.Equals(captured) ||
                call.Call.FunctionType is not { } signature || !signature.Equals(captured.Signature) ||
                !call.Type.Equals(captured.ReturnType) || call.Receiver is not null || call.Call.ReturnSlot is not null ||
                captured.Signature.IsVariadic || call.Arguments.Count != captured.Signature.Parameters.Count ||
                call.Operands.Count != call.Arguments.Count + 1 || !call.Operands[0].Equals(call.Callee))
                throw Limit("A managed-bound call requires its exact retained ordinary direct declaration and execution operands.", "managed-call");
            ValidateOrdinaryRoutineRoles(captured.Signature);
            ValidateOrdinaryRoutineRoles(selected.Signature);
            for (var index = 0; index < call.Arguments.Count; ++index)
            {
                var argument = call.Arguments[index]; var parameter = captured.Signature.Parameters[index];
                if (argument.Ordinal != index || argument.Role?.Name != parameter.Role.Name ||
                    argument.Parameter is null || !argument.Parameter.Equals(parameter.Descriptor) ||
                    !call.Operands[index + 1].Equals(argument.Value))
                    throw Limit("A managed-bound call argument must alias its actual declaration's selected parameter and raw operand.", "managed-call");
                ValidateProgramBoundaryType(parameter.Type);
                ValidateProgramBoundaryType(selected.Signature.Parameters[index].Type);
                var actual = new AbiCompatibility().CompareTypes(argument.Value.Type, parameter.Type);
                if (!actual.Compatible)
                    throw Limit($"A managed-bound occurrence requires its captured declaration ABI at {actual.Path}: {actual.Reason}", "managed-call");
            }
            ValidateProgramBoundaryType(captured.ReturnType);
            ValidateProgramBoundaryType(selected.ReturnType);
            declaration = captured;
            return true;
        }

        private bool TryValidateManagedBoundConstructorCall(CallOperation call, FunctionSymbol definition)
        {
            if (options.ManagedProgramSelection is not { } program ||
                call.Callee is not ReferenceOperation { Target: FunctionSymbol declaration } ||
                !program.FunctionDefinitions.TryGetValue(declaration, out var selected) ||
                declaration.SpecialFunctionKind?.Name != "sfk_constructor" && selected.SpecialFunctionKind?.Name != "sfk_constructor")
                return false;
            if (!selected.Equals(definition) || currentFunction is null || !functions.Contains(currentFunction) || !functions.Contains(definition) ||
                call.CallKind.Name != "direct" || call.TargetMethod?.Equals(declaration) != true ||
                call.Call.FunctionType?.Equals(declaration.Signature) != true || !call.Type.Equals(declaration.ReturnType) ||
                call.Operator.RetainedVirtualCall || call.Operator.RequiresRuntimeCastCheck ||
                call.Operator.EvalLeftToRight && call.Operator.EvalRightToLeft || call.Call.ReturnSlot is not null ||
                call.Arguments.Count != 1 || call.Operands.Count != 2 || !call.Operands[0].Equals(call.Callee))
                throw Limit("A managed-bound constructor requires its exact retained direct declaration, receiver and execution signature.", "managed-lifetime-call");

            var declarationRecord = ValidateManagedBoundConstructorEntry(declaration);
            _ = ValidateManagedBoundConstructorEntry(definition);
            var argument = call.Arguments[0]; var parameter = declaration.Signature.Parameters[0];
            if (argument.Ordinal != 0 || argument.Role?.Name != "this_parameter" ||
                argument.Parameter?.Equals(parameter.Descriptor) != true || !call.Operands[1].Equals(argument.Value) ||
                call.Receiver?.Equals(argument.Value) != true)
                throw Limit("A managed-bound constructor receiver must alias its original declaration descriptor, raw operand and receiver facet.", "managed-lifetime-call");
            if (!GeneratedRecordPointer(argument.Value.Type, false).Equals(declarationRecord))
                throw Limit("A managed-bound constructor occurrence must retain its declaration's own receiver record view.", "managed-lifetime-call");
            ValidateProgramBoundaryType(argument.Value.Type);
            var compatibility = new AbiCompatibility().CompareFunctions(declaration, definition);
            if (!compatibility.Compatible)
                throw Limit($"A managed-bound constructor requires the selected captured ABI at {compatibility.Path}: {compatibility.Reason}", "managed-lifetime-call");

            // Entry binding supplies only an occurrence-specific address view.
            // It grants no constructor body, cleanup or noexcept admission.
            ValidateGeneratedLifetimeRoutine(definition);
            return true;
        }

        private static RecordType ValidateManagedBoundConstructorEntry(FunctionSymbol function)
        {
            var signature = function.Signature;
            if (function.SpecialFunctionKind?.Name != "sfk_constructor" || function.ConstructorOrDestructorKind?.Name != "cdk_complete" ||
                function.PrimaryEntry is not null || function.Details.IsAliasEntry == true ||
                function.ThisAdjustment != 0 || function.ReturnAdjustment != 0 || function.VirtualCallIndex != 0 || function.VirtualBaseIndex != 0 ||
                !signature.HasPrototype || signature.IsVariadic || !signature.HasThisParameter ||
                signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor ||
                signature.ReturnValueParameterFollowsThis == true || MemoryRepresentationType(function.ReturnType).Kind.Name != "tk_void" ||
                signature.Parameters.Count != 1 || signature.Parameters[0].Ordinal != 0 || signature.Parameters[0].Role.Name != "this_parameter" ||
                signature.Parameters[0].BaseSubobject is not null || signature.Parameters[0].Descriptor.Details.PassedViaCopyConstructor ||
                signature.ThisClass is null || function.ContainingType is null)
                throw Limit("The managed-bound lifetime entry supports a complete receiver-only constructor without adjustment or hidden copy/result roles.", "managed-lifetime-call");
            ValidateCallingConvention(signature);
            var receiverType = signature.Parameters[0].Type;
            ValidateProgramBoundaryType(receiverType);
            var record = GeneratedRecordPointer(receiverType, false);
            if (!record.Equals(MemoryRepresentationType(signature.ThisClass)) || !record.Equals(MemoryRepresentationType(function.ContainingType)))
                throw Limit("A managed-bound constructor's receiver must retain its own selected containing record.", "managed-lifetime-call");
            return record;
        }

        private string ProgramArgumentView(CallOperation call, int index, FunctionSymbol definition, string value)
        {
            if (!IsManagedBoundCall(call, definition, out _))
                return ProgramArgumentView(call.Arguments[index].Value.Type, definition.Signature.Parameters[index].Type, value);
            return ManagedBoundValueView(call.Arguments[index].Value.Type, definition.Signature.Parameters[index].Type, value);
        }

        private string ProgramResultView(CallOperation call, FunctionSymbol definition, string value)
        {
            if (!IsManagedBoundCall(call, definition, out _)) return ProgramResultView(definition.ReturnType, call.Type, value);
            return ManagedBoundValueView(definition.ReturnType, call.Type, value);
        }

        private string ManagedBoundValueView(CppType actualType, CppType expectedType, string value)
        {
            // Called only for the actual direct occurrence after declaration,
            // selected definition and ABI/operand-role checks above. This does
            // not install any type-wide pointer equivalence or storage access.
            var actual = Unwrap(actualType); var expected = Unwrap(expectedType);
            if (actual is not PointerType actualPointer || expected is not PointerType expectedPointer) return value;
            ValidateProgramPointer(actualPointer); ValidateProgramPointer(expectedPointer);
            ValidateProgramBoundaryType(actualType); ValidateProgramBoundaryType(expectedType);
            var comparison = new AbiCompatibility().CompareTypes(actualType, expectedType);
            if (!comparison.Compatible)
                throw Limit($"A managed-bound pointer occurrence has no captured ABI view at {comparison.Path}: {comparison.Reason}", "managed-call");
            var expectedName = TypeName(expectedType);
            return TypeName(actualType) == expectedName ? value : $"unchecked(({expectedName})({value}))";
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
