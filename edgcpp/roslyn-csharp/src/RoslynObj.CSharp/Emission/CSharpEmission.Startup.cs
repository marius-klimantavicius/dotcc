using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record StartupRegistration(FunctionSymbol Owner, FunctionSymbol Runtime,
            FunctionSymbol Destructor, VariableSymbol Object, VariableSymbol Dso);
        private readonly List<FunctionSymbol> selectedStartupRoots = [];
        private readonly HashSet<FunctionSymbol> selectedCallableRoots = [];
        private readonly HashSet<FunctionSymbol> startupClosedBodies = [];
        private readonly HashSet<FunctionSymbol> startupTeardownBodies = [];
        private readonly Dictionary<CallOperation, StartupRegistration> startupRegistrations = [];
        private readonly Dictionary<FunctionSymbol, string> startupDestructorThunks = [];
        private readonly HashSet<VariableSymbol> startupDsoVariables = [];
        private readonly Dictionary<ExpressionOperation, HashSet<FunctionSymbol>> startupDsoOccurrences = [];
        private readonly Dictionary<FunctionSymbol, string> startupAdvertisedNames = [];
        private string? startupLifetimeField;
        private string? startupRunner;
        private string? startupDsoStorage;

        private bool SelectedStartupEnabled => options.ManagedStartup is not null;

        private void InitializeSelectedStartupSelection()
        {
            if (options.ManagedStartup is not { } configuration) return;
            var plans = options.ManagedProgramSelection?.Units ??
                (options.ManagedSelection is { } single ? new[] { single } : []);
            if (configuration.Contract != "edg7-itanium-explicit-startup-atexit-v1" || selectedProgram is not null ||
                plans.Count != inputs.Count || plans.Where((plan, index) => !ReferenceEquals(plan.Source, inputs[index]) ||
                    !plan.Report.ClosureComplete || plan.Report.StartupExecutionContract != configuration.Contract).Any() ||
                !configuration.UnitInputOrder.SequenceEqual(plans.Select(plan => plan.Report.InputSha256), StringComparer.OrdinalIgnoreCase))
                throw Limit("Explicit startup requires its completed current-reader selection and captured-input order contract.", "selected-startup");
            if (options.Executable)
                throw Limit("Explicit startup currently requires a library host to call InitializeSelectedProgram and ShutdownSelectedProgram.", "selected-startup");
            foreach (var plan in plans)
            {
                selectedStartupRoots.AddRange(plan.StartupDefinitions.Select(CanonicalFunction));
                foreach (var callable in plan.CallableDefinitions) selectedCallableRoots.Add(CanonicalFunction(callable));
            }
            if (selectedStartupRoots.Count != configuration.Definitions.Length || selectedStartupRoots.Distinct().Count() != selectedStartupRoots.Count ||
                selectedStartupRoots.Any(selectedCallableRoots.Contains))
                throw Limit("The startup schedule must preserve its exact distinct definitions, separately from advertised callable roots.", "selected-startup");
            globalNames.Add("InitializeSelectedProgram");
            globalNames.Add("ShutdownSelectedProgram");
            globalNames.Add("SelectedProgramDiagnostics");
            startupLifetimeField = InternalName("__selected_program_lifetime");
            startupRunner = InternalName("__RunSelectedStartup");
            startupDsoStorage = InternalName("__selected_program_dso");
        }

        private void InitializeSelectedStartup()
        {
            if (!SelectedStartupEnabled) return;
            foreach (var root in selectedStartupRoots)
            {
                if (!functions.Contains(root) || root.Parameters.Count != 0 || root.Signature.HasThisParameter ||
                    root.Signature.ReturnsValueAsParameter == true || root.Signature.ReturnsValueViaCopyConstructor ||
                    MemoryRepresentationType(root.ReturnType).Kind.Name != "tk_void")
                    throw Limit("A startup definition requires its selected fixed void function without execution parameters.", "selected-startup");
                ValidateStartupClosedFunction(root, []);
            }
            foreach (var registration in startupRegistrations.Values)
            {
                if (!startupDestructorThunks.ContainsKey(registration.Destructor))
                    startupDestructorThunks.Add(registration.Destructor, InternalName("__selected_startup_destructor"));
                startupDsoVariables.Add(registration.Dso);
            }
            foreach (var function in selectedCallableRoots)
            {
                if (!functions.Contains(function) || function.Signature.IsVariadic)
                    throw Limit("An advertised startup-program root requires its selected fixed generated body.", "selected-startup");
                startupAdvertisedNames.Add(function, names[function]);
                names[function] = InternalName("__body_" + names[function].TrimStart('@'));
            }
            foreach (var function in functions)
            {
                foreach (var node in ExceptionExecutionNodes(function.Body!.Root))
                {
                    if (node is CallOperation call && call.Callee is ReferenceOperation { Target: FunctionSymbol called } &&
                        called.CurrentName == "__cxa_atexit" && !startupRegistrations.ContainsKey(call))
                        throw Limit("An exit registration requires its actual selected startup owner and complete callback/object/DSO contract.", "selected-startup");
                    if (node is ReferenceOperation { Target: VariableSymbol variable } reference && startupDsoVariables.Contains(variable) &&
                        (!startupDsoOccurrences.TryGetValue(reference, out var owners) || !owners.Contains(function)))
                        throw Limit("The logical module handle supports only its exact registered address occurrence.", "selected-startup-dso");
                    if (node is ConstantOperation { Constant.Value: AddressValue { BaseEntity: VariableSymbol addressed } } && startupDsoVariables.Contains(addressed))
                        throw Limit("A logical module handle constant outside the selected registration address projection is unsupported.", "selected-startup-dso");
                    if (node is ReferenceOperation { Target: FunctionSymbol target } routine && startupDestructorThunks.ContainsKey(CanonicalFunction(target)) &&
                        !ExceptionExecutionNodes(function.Body!.Root).OfType<CallOperation>().Any(call => call.Callee.Equals(routine)) &&
                        !startupRegistrations.Keys.Any(call => ExceptionExecutionNodes(call.Arguments[0].Value).Contains(routine)))
                        throw Limit("A selected exit destructor has no general function-address or native callback adapter.", "selected-startup");
                    if (node is ConstantOperation { Constant.Value: AddressValue { BaseEntity: FunctionSymbol addressedFunction } } constant &&
                        startupDestructorThunks.ContainsKey(CanonicalFunction(addressedFunction)) &&
                        !startupRegistrations.Keys.Any(call => startupRegistrations[call].Owner.Equals(function) &&
                            ExceptionExecutionNodes(call.Arguments[0].Value).Contains(constant)))
                        throw Limit("A destructor address supports only its selected managed registration occurrence.", "selected-startup");
                }
            }
            var plans = options.ManagedProgramSelection?.Units ?? (options.ManagedSelection is { } single ? new[] { single } : []);
            if (plans.SelectMany(plan => plan.Variables).Any(variable => variable.CurrentName == "__dso_handle" && !startupDsoVariables.Contains(variable)))
                throw Limit("Every required module-handle declaration needs its complete selected registration address contract.", "selected-startup-dso");
            foreach (var item in storageOrder)
                if (item.Initializer is { } initializer) ValidateStartupStoredValue(initializer.Value, []);
            foreach (var runtime in startupRegistrations.Values.Select(registration => registration.Runtime).Distinct())
                semanticRuntimeFunctions.Add(new(runtime, SemanticRuntimeFunctionImplementation.SelectedStartupRegistrationContract));
        }

        private void ValidateStartupStoredValue(ConstantValue value, HashSet<ConstantValue> active)
        {
            if (!active.Add(value)) throw Limit("A selected startup constant has a cyclic value graph.", "selected-startup");
            switch (value)
            {
                case AddressValue { BaseEntity: VariableSymbol variable } when startupDsoVariables.Contains(variable):
                    throw Limit("The logical module handle cannot escape through stored address constants.", "selected-startup-dso");
                case AddressValue { BaseEntity: FunctionSymbol function } when startupDestructorThunks.ContainsKey(CanonicalFunction(function)):
                    throw Limit("An exit destructor has no stored native function-address adapter.", "selected-startup");
                case AggregateValue aggregate:
                    foreach (var element in aggregate.Elements) ValidateStartupStoredValue(element.Value, active);
                    break;
                case ConstantValue<RepeatValueDetails> repeat: ValidateStartupStoredValue(repeat.Payload.Value, active); break;
                case IntegerValue or FloatingValue or StringValue or AddressValue: break;
                default: throw Limit("The selected startup constant has an unsupported stored value graph.", "selected-startup");
            }
            active.Remove(value);
        }

        private void ValidateStartupClosedFunction(FunctionSymbol function, HashSet<FunctionSymbol> active, bool teardown = false)
        {
            function = CanonicalFunction(function);
            if (teardown ? startupTeardownBodies.Contains(function) : startupClosedBodies.Contains(function)) return;
            if (!active.Add(function)) throw Limit("Recursive selected startup propagation is outside the bounded closed-body profile.", "selected-startup");
            if (!functions.Contains(function) || function.Body is not { } body ||
                function.Signature.IsVariadic || !function.Signature.HasPrototype || function.IsVirtual || function.IsPureVirtual ||
                function.Details.IsWeak == true || function.Details.IsWeakref == true || function.Details.IsGnuAlias == true ||
                function.Details.IsIfunc == true || function.ThisAdjustment != 0 || function.ReturnAdjustment != 0 ||
                function.VirtualCallIndex != 0 || function.VirtualBaseIndex != 0 || function.OverridingFunctionForWrapper is not null ||
                function.OverriddenFunctionForWrapper is not null)
                throw Limit("Startup requires a closed selected fixed generated body without dynamic/native dispatch.", "selected-startup");
            ValidateCallingConvention(function.Signature);
            var eh = body.ExceptionHandling;
            if (body.Lifetimes.Count != 0 || eh.RegionTable is not null || eh.ArrayTable is not null || eh.CleanupRegions.Count != 0 ||
                eh.Arrays.Count != 0 || eh.Details?.Prologue is not null || eh.Epilogues.Count != 0 ||
                eh.Details?.InitialState is not null || eh.Details?.NullState is not null)
                throw Limit("Startup body admission does not implement retained lifetime or exceptional cleanup schedules.", "selected-startup");
            var nodes = ExceptionExecutionNodes(body.Root).ToArray();
            if (nodes.Any(node => node.RepresentationRole.Name != "execution" ||
                    node is ThrowOperation or TryOperation or AllocationOperation or LoweredEhOperation or LifetimeOperation) ||
                body.Locals.Any(local => local.IsThreadLocal || local.LocalStaticInitializations.Count != 0))
                throw Limit("Startup requires supported nonthrowing retained execution without allocation, TLS or local-static initialization.", "selected-startup");
            foreach (var call in nodes.OfType<CallOperation>())
            {
                if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target, Operands.Count: 0 } ||
                    call.TargetMethod?.Equals(target) != true || call.CallKind.Name != "direct" ||
                    call.Call.FunctionType?.Equals(target.Signature) != true || call.Operator.RetainedVirtualCall ||
                    call.Operator.RequiresRuntimeCastCheck || !call.Operator.EvalLeftToRight || call.Operator.EvalRightToLeft ||
                    call.Arguments.Count != target.Signature.Parameters.Count || call.Operands.Count != call.Arguments.Count + 1 ||
                    !call.Operands[0].Equals(call.Callee))
                    throw Limit("Startup calls require their actual selected direct signatures, aliases and retained left-to-right order.", "selected-startup");
                for (var index = 0; index < call.Arguments.Count; ++index)
                    if (call.Arguments[index].Ordinal != index || !call.Arguments[index].Value.Equals(call.Operands[index + 1]) ||
                        call.Arguments[index].Parameter?.Equals(target.Signature.Parameters[index].Descriptor) != true ||
                        call.Arguments[index].Role?.Equals(target.Signature.Parameters[index].Role) != true)
                        throw Limit("A startup call changes its selected execution argument role or raw operand occurrence.", "selected-startup");
                if (target.CurrentName == "__cxa_atexit")
                {
                    if (teardown)
                        throw Limit("Registering an exit callback from the actual teardown call graph requires a separate lifecycle contract.", "selected-startup");
                    ValidateStartupRegistration(function, call, target, active);
                    continue;
                }
                if (target.BuiltinKind is not null)
                    throw Limit("This startup profile has no additional builtin execution contract.", "selected-startup");
                ValidateStartupClosedFunction(CanonicalFunction(target), active, teardown);
            }
            active.Remove(function);
            startupClosedBodies.Add(function);
            if (teardown) startupTeardownBodies.Add(function);
        }

        private void ValidateStartupRegistration(FunctionSymbol owner, CallOperation call, FunctionSymbol runtime, HashSet<FunctionSymbol> active)
        {
            var signature = runtime.Signature;
            if (runtime.Body is not null || runtime.BodyState.Name != "declaration_only" || runtime.PrimaryEntry is not null ||
                runtime.AlternateEntryPoints.Count != 0 || runtime.Details.IsWeak == true || runtime.Details.IsWeakref == true ||
                runtime.Details.IsGnuAlias == true || runtime.Details.IsIfunc == true || runtime.Details.IsAliasEntry == true ||
                runtime.GnuSupplement?.AliasedRoutine is not null || runtime.GnuSupplement?.TargetedVersions.Count > 0 ||
                runtime.GnuSupplement?.Representative is not null || !signature.HasPrototype || signature.IsVariadic ||
                signature.HasThisParameter || signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor ||
                signature.ReturnValueParameterFollowsThis == true ||
                signature.Parameters.Count != 3 || signature.Parameters.Any(parameter => parameter.Role.Name != "abi_parameter" ||
                    parameter.Descriptor.Details.PassedViaCopyConstructor || parameter.BaseSubobject is not null) ||
                call.Arguments.Count != 3 || call.Receiver is not null || call.Call.ReturnSlot is not null ||
                !call.Type.Equals(runtime.ReturnType) || MemoryRepresentationType(runtime.ReturnType) is not ArithmeticType { Arithmetic.Bits: 32, Arithmetic.Signed: true } ||
                runtime.ReturnType.Size != 4 || runtime.ReturnType.Alignment != 4 ||
                options.ExternalFunctions.Any(binding => binding.LinkageName == runtime.CurrentName))
                throw Limit("The selected exit registration has no matching generated Itanium callback/object/DSO contract.", "selected-startup");
            if (MemoryRepresentationType(signature.Parameters[0].Type) is not PointerType { IsReference: false, IsRValueReference: false } callbackPointer ||
                MemoryRepresentationType(callbackPointer.PointeeType) is not RoutineType callback ||
                callbackPointer.Size != 8 || callbackPointer.Alignment != 8 || !callback.HasPrototype || callback.IsVariadic || callback.HasThisParameter ||
                callback.ReturnsValueAsParameter == true || callback.ReturnsValueViaCopyConstructor || callback.Parameters.Count != 1 ||
                MemoryRepresentationType(callback.ReturnType).Kind.Name != "tk_void" ||
                MemoryRepresentationType(callback.Parameters[0].Type) is not PointerType { IsReference: false, IsRValueReference: false } callbackObject ||
                MemoryRepresentationType(callbackObject.PointeeType).Kind.Name != "tk_void" || callbackObject.Size != 8 || callbackObject.Alignment != 8)
                throw Limit("The exit callback requires its captured natural void(void*) ABI descriptor.", "selected-startup");
            ValidateCallingConvention(callback);
            ValidateCallingConvention(signature);
            foreach (var argument in call.Arguments)
                if (MemoryRepresentationType(argument.Value.Type) is not PointerType { IsReference: false, IsRValueReference: false } actual ||
                    argument.Value.RepresentationRole.Name != "execution" || argument.Value.ValueCategory.Name != "prvalue" ||
                    argument.Value.Type.Size != 8 || argument.Value.Type.Alignment != 8 || actual.Size != 8 || actual.Alignment != 8)
                    throw Limit("Each exit registration address operand requires its captured natural pointer value.", "selected-startup");
            foreach (var parameter in signature.Parameters.Skip(1))
                if (MemoryRepresentationType(parameter.Type) is not PointerType { IsReference: false, IsRValueReference: false } pointer ||
                    MemoryRepresentationType(pointer.PointeeType).Kind.Name != "tk_void" || pointer.Size != 8 || pointer.Alignment != 8)
                    throw Limit("The registration object and DSO descriptors require natural captured void pointers.", "selected-startup");
            var destructorExpression = StartupAddressLeaf(call.Arguments[0].Value);
            if (destructorExpression is not ConstantOperation { Constant.Value: AddressValue { BaseEntity: FunctionSymbol rawDestructor } address } ||
                address.ByteOffset != 0 || address.OnePastEnd || address.SubobjectPath is not null)
                throw Limit("The exit callback requires an effect-free captured selected routine address, without native address materialization.", "selected-startup");
            if (rawDestructor.SpecialFunctionKind?.Name != "sfk_destructor" || rawDestructor.Parameters.Count != 1 ||
                rawDestructor.Parameters[0].Role.Name != "this_parameter" || MemoryRepresentationType(rawDestructor.ReturnType).Kind.Name != "tk_void")
                throw Limit("The selected exit callback requires its retained ordinary destructor receiver.", "selected-startup");
            var objectExpression = StartupAddressLeaf(call.Arguments[1].Value);
            if (objectExpression is not ConstantOperation { Constant.Value: AddressValue { BaseEntity: VariableSymbol rawObject } objectAddress } ||
                objectAddress.ByteOffset != 0 || objectAddress.OnePastEnd || objectAddress.SubobjectPath is not null)
                throw Limit("The registered object requires its actual zero-offset selected static storage address.", "selected-startup");
            var selectedObject = CanonicalVariable(rawObject);
            if (!storage.ContainsKey(selectedObject) || selectedObject.IsThreadLocal || selectedObject.Details.StorageDuration?.Name != "static" ||
                !GeneratedRecordPointer(rawDestructor.Parameters[0].Type, false).Equals(MemoryRepresentationType(selectedObject.Type)))
                throw Limit("The exit callback receiver must match its actual selected object storage and record type.", "selected-startup");
            var destructor = SelectedLifetimeInvocation(rawDestructor);
            var dsoExpression = StartupAddressLeaf(call.Arguments[2].Value);
            if (dsoExpression is not OperatorOperation { OperatorKind.Name: "eok_address_of", Operands.Count: 1 } dsoAddress ||
                dsoAddress.ValueCategory.Name != "prvalue" ||
                dsoAddress.Operands[0] is not ReferenceOperation { Target: VariableSymbol dso, Operands.Count: 0 } dsoReference ||
                dsoReference.ValueCategory.Name != "lvalue" || !dsoReference.Type.Equals(dso.Type) ||
                dso.CurrentName != "__dso_handle" || dso.SymbolDetails.Storage?.Name != "sc_extern" ||
                dso.Type.Size != 8 || dso.Type.Alignment != 8 || MemoryRepresentationType(dso.Type) is not PointerType { IsReference: false, IsRValueReference: false } dsoPointer ||
                MemoryRepresentationType(dsoPointer.PointeeType).Kind.Name != "tk_void" || dso.IsThreadLocal ||
                dso.Initializer is not null || dso.ConstantInitializer is not null || dso.Details.IsWeak || dso.Details.IsGnuAlias || dso.Details.IsWeakref ||
                dso.Details.AliasedVariable is not null || !CanonicalVariable(dso).Equals(dso) ||
                options.ExternalData.Any(binding => binding.LinkageName == dso.CurrentName))
                throw Limit("The module handle requires the exact external natural8/8 address-only __dso_handle role.", "selected-startup-dso");
            ValidateStartupClosedFunction(destructor, active, teardown: true);
            foreach (var expression in ExceptionExecutionNodes(call.Arguments[2].Value).OfType<ExpressionOperation>())
            {
                if (!startupDsoOccurrences.TryGetValue(expression, out var owners)) startupDsoOccurrences.Add(expression, owners = []);
                owners.Add(owner);
            }
            var registration = new StartupRegistration(owner, runtime, destructor, selectedObject, dso);
            if (startupRegistrations.TryGetValue(call, out var previous) && !previous.Equals(registration))
                throw Limit("An exit registration occurrence has different retained owners or selected storage.", "selected-startup");
            startupRegistrations[call] = registration;
        }

        private static ExpressionOperation StartupAddressLeaf(ExpressionOperation expression)
        {
            foreach (var node in ExceptionExecutionNodes(expression))
                if (node.RepresentationRole.Name != "execution" || node is ExpressionOperation { Details.VolatileFetch: true } ||
                    node is OperatorOperation operation && (operation.Operator.EvalLeftToRight || operation.Operator.EvalRightToLeft ||
                        operation.Operator.RequiresRuntimeCastCheck || operation.Operator.RetainedVirtualCall))
                    throw Limit("A startup registration address must retain its effect-free ordinary execution wrappers.", "selected-startup");
            for (var depth = 0; expression is ConversionOperation { OperatorKind.Name: "eok_cast", Operands.Count: 1 } conversion; ++depth)
            {
                if (depth >= 16) throw Limit("A startup registration address wrapper exceeds the supported bound.", "selected-startup");
                expression = conversion.Operand;
            }
            return expression;
        }

        private bool TryValidateStartupLifetimeRoutine(FunctionSymbol function)
        {
            if (!SelectedStartupEnabled || !startupClosedBodies.Contains(function) ||
                function.SpecialFunctionKind?.Name is not ("sfk_constructor" or "sfk_destructor")) return false;
            var signature = function.Signature;
            if (!signature.HasThisParameter || signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor ||
                MemoryRepresentationType(function.ReturnType).Kind.Name != "tk_void" || function.Parameters.Count == 0 ||
                function.Parameters[0].Role.Name != "this_parameter" || function.ContainingType is not RecordType record ||
                signature.ThisClass?.Equals(record) != true || !GeneratedRecordPointer(function.Parameters[0].Type, false).Equals(record) ||
                function.Parameters.Skip(1).Any(parameter => parameter.Role.Name is not ("ordinary_parameter" or "abi_parameter") ||
                    MemoryRepresentationType(parameter.Type) is not ArithmeticType))
                throw Limit("The selected startup lifetime method requires its actual ordinary receiver and natural arithmetic execution roles.", "selected-startup");
            foreach (var parameter in function.Parameters.Skip(1)) ValidateNativeScalarType(parameter.Type, false);
            return true;
        }

        private bool TryPlanStartupRegistration(CallOperation call, out ExpressionPlan result)
        {
            result = null!;
            if (!startupRegistrations.TryGetValue(call, out var registration)) return false;
            if (!registration.Owner.Equals(currentFunction)) throw Limit("An exit registration escaped its selected startup owner.", "selected-startup");
            // Callback and DSO operands are validated effect-free address roles;
            // neither materializes a native function pointer. The object operand
            // still goes through the ordinary typed value planner once.
            var value = PlanExpression(call.Arguments[1].Value);
            var text = $"{Owner}.{startupLifetimeField}.Register({Owner}.{startupDestructorThunks[registration.Destructor]}, " +
                $"(nint)({RenderExpression(value.Value)}), (nint){Owner}.{startupDsoStorage}.Pointer)";
            result = new(call.Type, call.ValueCategory, value.Prefix, new CsLeaf(CsType.Value(call.Type), text, false),
                value.Effects | ExpressionEffects.Call | ExpressionEffects.MayThrow);
            return true;
        }

        private bool TrySelectedStartupDsoReference(ReferenceOperation reference, out string value)
        {
            value = "";
            if (reference.Target is not VariableSymbol variable || !startupDsoVariables.Contains(variable)) return false;
            if (currentFunction is null || !startupDsoOccurrences.TryGetValue(reference, out var owners) || !owners.Contains(currentFunction))
                throw Limit("The module-handle address is outside its selected registration occurrence.", "selected-startup-dso");
            value = $"(*(({TypeName(variable.Type)}*){Owner}.{startupDsoStorage}.Pointer))";
            return true;
        }

        private void EmitSelectedStartupSupport(StringBuilder fields)
        {
            if (!SelectedStartupEnabled) return;
            fields.Append("    private static readonly global::RoslynObj.CSharp.Runtime.ManagedSelectedProgramLifetime ")
                .Append(startupLifetimeField).AppendLine(" = new();");
            if (startupDsoVariables.Count != 0)
                fields.Append("    private static readonly global::RoslynObj.CSharp.Runtime.PinnedStorage ")
                    .Append(startupDsoStorage).AppendLine(" = new(8, 8);");
            methods.AppendLine("    public static void InitializeSelectedProgram()")
                .Append("        => ").Append(startupLifetimeField).Append(".Initialize(").Append(startupRunner).AppendLine(");").AppendLine();
            methods.AppendLine("    public static void ShutdownSelectedProgram()")
                .Append("        => ").Append(startupLifetimeField).AppendLine(".Shutdown();").AppendLine();
            methods.AppendLine("    public static global::RoslynObj.CSharp.Runtime.ManagedSelectedProgramLifetimeDiagnostics SelectedProgramDiagnostics()")
                .Append("        => ").Append(startupLifetimeField).AppendLine(".GetDiagnostics();").AppendLine();
            methods.Append("    private static void ").Append(startupRunner).AppendLine("()\n    {");
            foreach (var function in selectedStartupRoots) methods.Append("        ").Append(OwnedName(function)).AppendLine("();");
            methods.AppendLine("    }").AppendLine();
            foreach (var thunk in startupDestructorThunks)
                methods.Append("    private static void ").Append(thunk.Value).AppendLine("(nint address)\n    {")
                    .Append("        ").Append(OwnedName(thunk.Key)).Append("((").Append(TypeName(thunk.Key.Parameters[0].Type)).AppendLine(")address);")
                    .AppendLine("    }").AppendLine();
            foreach (var advertised in startupAdvertisedNames)
            {
                var function = advertised.Key;
                var previous = currentFunction; currentFunction = function;
                try
                {
                    var result = TypeName(function.ReturnType);
                    var parameters = function.Parameters.Select((parameter, index) => $"{TypeName(parameter.Type)} argument{index}");
                    methods.Append("    public static ").Append(result).Append(' ').Append(advertised.Value)
                        .Append('(').Append(string.Join(", ", parameters)).AppendLine(")\n    {")
                        .Append("        ").Append(startupLifetimeField).AppendLine(".RequireInitialized();")
                        .Append("        ").Append(result == "void" ? "" : "return ").Append(OwnedName(function)).Append('(')
                        .Append(string.Join(", ", function.Parameters.Select((_, index) => "argument" + index))).AppendLine(");")
                        .AppendLine("    }").AppendLine();
                }
                finally { currentFunction = previous; }
            }
        }
    }
}
