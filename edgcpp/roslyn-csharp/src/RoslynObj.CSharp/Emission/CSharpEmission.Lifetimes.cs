using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private readonly HashSet<FunctionSymbol> generatedLifetimeRoutines = [];
        private readonly HashSet<PointerType> generatedReferenceTypes = [];
        private readonly HashSet<FunctionSymbol> normalCleanupFunctions = [];
        private readonly HashSet<LoweredEhOperation> normalCleanupMarkers = [];
        private readonly HashSet<LifetimeOperation> normalLifetimeExpressions = [];
        private readonly Dictionary<FunctionSymbol, VariableSymbol> omittedNormalRegionTables = [];
        private sealed record ReceiverMemberCleanup(FunctionSymbol Function, Initialization Recipe, VariableSymbol Receiver, FieldSymbol Field, RecordType MemberType);
        private readonly Dictionary<CleanupAction, ReceiverMemberCleanup> receiverMemberCleanups = [];

        private void RequireNormalLifetimeProfile()
        {
            if (inputs.Count != 1 || selectedProgram is not null || inputs[0].Manifest.CaptureProfile != "native_lowered" ||
                inputs[0].Manifest.Stage != "lowered" || !inputs[0].TranslationUnit.LanguageMode.StartsWith("c++", StringComparison.Ordinal))
                throw Limit("The generated normal-lifetime adapter requires one native-lowered unit.", "lifetime");
        }

        private static bool NeedsGeneratedLifetimeRoles(RoutineType signature) => signature.HasThisParameter ||
            signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor ||
            MemoryRepresentationType(signature.ReturnType) is PointerType { IsReference: true } ||
            signature.Parameters.Any(parameter => parameter.Role.Name != "ordinary_parameter" ||
                MemoryRepresentationType(parameter.Type) is PointerType { IsReference: true });

        private void ValidateGeneratedDefinitionRoles(FunctionSymbol function)
        {
            if (!NeedsGeneratedLifetimeRoles(function.Signature))
            {
                ValidateOrdinaryRoutineRoles(function.Signature);
                if (function.Parameters.Any(parameter => parameter.Role.Name != "ordinary_parameter"))
                    throw Limit("An execution parameter has no selected generated role adapter.", "parameter");
                return;
            }
            ValidateGeneratedLifetimeRoutine(function);
        }

        private void ValidateGeneratedLifetimeRoutine(FunctionSymbol function)
        {
            if (generatedLifetimeRoutines.Contains(function)) return;
            RequireNormalLifetimeProfile();
            var signature = function.Signature;
            var parameters = function.Parameters;
            if (!functions.Contains(function) || function.Body is null || !signature.HasPrototype || signature.IsVariadic ||
                parameters.Count != signature.Parameters.Count || parameters.Any(parameter => parameter.Symbol is null ||
                    parameter.BaseSubobject is not null || parameter.Descriptor.Details.PassedViaCopyConstructor))
                throw Limit("Generated lifetime routines require selected fixed execution parameters without implicit copy adaptation.", "lifetime-parameter");
            ValidateCallingConvention(signature);
            for (var index = 0; index < parameters.Count; ++index)
            {
                var parameter = parameters[index];
                var selected = signature.Parameters[index];
                if (parameter.Ordinal != index || selected.Ordinal != index || !parameter.Descriptor.Equals(selected.Descriptor) ||
                    !parameter.Role.Equals(selected.Role) || selected.BaseSubobject is not null)
                    throw Limit("Execution and signature parameter roles differ in the selected lifetime routine.", "lifetime-parameter");
            }
            var references = new List<PointerType>();
            if (signature.HasThisParameter)
            {
                if (MemoryRepresentationType(function.ReturnType).Kind.Name != "tk_void" ||
                    signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor ||
                    function.SpecialFunctionKind?.Name is not ("sfk_constructor" or "sfk_destructor") ||
                    function.ConstructorOrDestructorKind?.Name is not ("cdk_complete" or "cdk_subobject") ||
                    parameters.Count == 0 || parameters[0].Role.Name != "this_parameter" ||
                    signature.ThisClass is null || function.ContainingType is null)
                    throw Limit("Only retained ordinary constructor/destructor receiver roles are admitted.", "lifetime-parameter");
                var record = GeneratedRecordPointer(parameters[0].Type, false);
                if (!record.Equals(MemoryRepresentationType(signature.ThisClass)) || !record.Equals(MemoryRepresentationType(function.ContainingType)))
                    throw Limit("The selected receiver pointer does not refer to its retained containing record.", "lifetime-parameter");
                var destructor = function.SpecialFunctionKind?.Name == "sfk_destructor";
                if (destructor ? parameters.Count != 1 : parameters.Count != 2)
                    throw Limit("The exercised generated constructor/destructor has one receiver and at most one source argument.", "lifetime-parameter");
                if (!destructor)
                {
                    var source = parameters[1];
                    var alternate = function.ConstructorOrDestructorKind?.Name == "cdk_subobject";
                    if (source.Role.Name != (alternate ? "abi_parameter" : "ordinary_parameter"))
                        throw Limit("A constructor source argument has an unexercised selected role.", "lifetime-parameter");
                    if (MemoryRepresentationType(source.Type) is PointerType { IsReference: true } reference)
                    {
                        if (!GeneratedRecordPointer(source.Type, true).Equals(record))
                            throw Limit("The selected copy source references a different record.", "lifetime-parameter");
                        references.Add(reference);
                    }
                    else ValidateGeneratedInt32(source.Type);
                }
                if (function.ConstructorOrDestructorKind?.Name == "cdk_subobject")
                    ValidateGeneratedAlternateBody(function);
                else if (signature.Routine.ExceptionSpecification?.NoexceptEffective != true && !validatedThrowingConstructors.Contains(function))
                    throw Limit("The first generated constructor/destructor adapter requires the retained noexcept specification.", "lifetime");
            }
            else if (signature.ReturnsValueAsParameter == true)
            {
                if (!signature.ReturnsValueViaCopyConstructor || signature.ReturnValueParameterFollowsThis == true ||
                    MemoryRepresentationType(function.ReturnType).Kind.Name != "tk_void" || parameters.Count != 2 ||
                    parameters[0].Role.Name != "return_parameter" || parameters[1].Role.Name != "ordinary_parameter" ||
                    signature.Routine.ExceptionSpecification?.NoexceptEffective != true)
                    throw Limit("The exercised generated copy result is an explicit first result pointer followed by one reference source.", "lifetime-parameter");
                var result = GeneratedRecordPointer(parameters[0].Type, false);
                if (!GeneratedRecordPointer(parameters[1].Type, true).Equals(result))
                    throw Limit("The selected result and copy-source records differ.", "lifetime-parameter");
                references.Add((PointerType)MemoryRepresentationType(parameters[1].Type));
            }
            else
            {
                if (signature.ReturnsValueViaCopyConstructor || parameters.Count != 1 || parameters[0].Role.Name != "ordinary_parameter" ||
                    signature.Routine.ExceptionSpecification?.NoexceptEffective != true)
                    throw Limit("The first ordinary generated reference adapter accepts one retained record reference and a scalar result.", "reference");
                _ = GeneratedRecordPointer(parameters[0].Type, true);
                ValidateGeneratedInt32(function.ReturnType);
                references.Add((PointerType)MemoryRepresentationType(parameters[0].Type));
            }
            foreach (var reference in references) generatedReferenceTypes.Add(reference);
            if (signature.ExceptionSpecification?.NoexceptEffective == true) EnsureGeneratedNoThrow(function);
            generatedLifetimeRoutines.Add(function);
        }

        private static void ValidateGeneratedInt32(CppType type)
        {
            ValidateAggregateQualifiers(type);
            if (MemoryRepresentationType(type) is not ArithmeticType arithmetic || arithmetic.Arithmetic.Bits != 32 ||
                arithmetic.Arithmetic.Signed != true || type.Size != 4 || type.Alignment != 4 || arithmetic.Size != 4 || arithmetic.Alignment != 4)
                throw Limit("This generated lifetime source/result requires the selected natural signed 32-bit value.", "lifetime-parameter");
        }

        private static RecordType GeneratedRecordPointer(CppType type, bool reference)
        {
            ValidateAggregateQualifiers(type);
            if (MemoryRepresentationType(type) is not PointerType pointer || pointer.IsReference != reference || pointer.IsRValueReference ||
                type.Size != 8 || type.Alignment != 8 || pointer.Size != 8 || pointer.Alignment != 8)
                throw Limit("The generated lifetime adapter requires its selected natural pointer or lvalue-reference representation.", "reference");
            if (reference && pointer.PointeeType is not QualifiedType { IsConst: true })
                throw Limit("The exercised generated record reference has its retained const-qualified pointee.", "reference");
            ValidateAggregateQualifiers(pointer.PointeeType);
            if (MemoryRepresentationType(pointer.PointeeType) is not RecordType record || record.IsIncomplete)
                throw Limit("The generated lifetime pointer requires a retained complete record layout.", "reference");
            return record;
        }

        private void ValidateGeneratedAlternateBody(FunctionSymbol function)
        {
            var primary = function.PrimaryEntry;
            if (primary is null || !functions.Contains(primary) || !primary.AlternateEntryPoints.Contains(function) ||
                primary.Signature.Routine.ExceptionSpecification?.NoexceptEffective != true && !validatedThrowingConstructors.Contains(primary) ||
                primary.SpecialFunctionKind != function.SpecialFunctionKind)
                throw Limit("The generated alternate entry requires its actual defined noexcept or validated throwing-constructor primary and reciprocal retained association.", "lifetime-entry");
            if (primary.Signature.ExceptionSpecification?.NoexceptEffective == true) EnsureGeneratedNoThrow(primary);
            else if (function.SpecialFunctionKind?.Name != "sfk_constructor")
                throw Limit("Only the selected throwing constructor has an exercised generated forwarding entry.", "lifetime-entry");
            var calls = NormalLifetimeNodes(function.Body!.Root).OfType<CallOperation>().ToArray();
            if (calls.Length != 1 || calls[0].Callee is not ReferenceOperation { Target: FunctionSymbol target } || !target.Equals(primary) ||
                calls[0].TargetMethod is null || !calls[0].TargetMethod!.Equals(primary) || calls[0].Arguments.Count != function.Parameters.Count)
                throw Limit("The exercised alternate entry retains one explicit primary-entry forwarding call.", "lifetime-entry");
            for (var index = 0; index < function.Parameters.Count; ++index)
                if (calls[0].Arguments[index].Value is not ReferenceOperation { Target: VariableSymbol argument } ||
                    !argument.Equals(function.Parameters[index].Symbol))
                    throw Limit("An alternate-entry call does not forward its actual selected execution parameters once.", "lifetime-entry");
        }

        private bool TryGeneratedReferenceType(PointerType pointer, out string result)
        {
            result = "";
            if (!pointer.IsReference || !generatedReferenceTypes.Contains(pointer) || currentFunction is not { } function ||
                !generatedLifetimeRoutines.Contains(function) && !normalCleanupFunctions.Contains(function)) return false;
            _ = GeneratedRecordPointer(pointer, true);
            result = TypeName(pointer.PointeeType) + "*";
            return true;
        }

        private void ValidateGeneratedCallRoles(CallOperation call)
        {
            var direct = call.Callee is ReferenceOperation { Target: FunctionSymbol target } ? CanonicalFunction(target) : null;
            if (call.Receiver is null && call.Call.ReturnSlot is null && (direct is null || !NeedsGeneratedLifetimeRoles(direct.Signature))) return;
            if (direct is null || !functions.Contains(direct) || call.TargetMethod is null || !CanonicalFunction(call.TargetMethod).Equals(direct))
                throw Limit("Generated receiver/result/reference calls require the actual selected direct body.", "lifetime-call");
            ValidateGeneratedLifetimeRoutine(direct);
            var signature = direct.Signature;
            var arguments = call.Arguments;
            var operands = call.Operands;
            if (arguments.Count != signature.Parameters.Count || operands.Count != arguments.Count + 1 || !operands[0].Equals(call.Callee) ||
                call.Call.FunctionType is not { } selectedType || !selectedType.Equals(signature) || !call.Type.Equals(direct.ReturnType))
                throw Limit("The selected generated call and its retained execution signature differ.", "lifetime-call");
            for (var index = 0; index < arguments.Count; ++index)
            {
                var argument = arguments[index];
                var parameter = signature.Parameters[index];
                if (argument.Ordinal != index || !operands[index + 1].Equals(argument.Value) || argument.Role?.Name != parameter.Role.Name ||
                    argument.Parameter is null || !argument.Parameter.Equals(parameter.Descriptor))
                    throw Limit("Call arguments must already contain each selected execution parameter in retained order.", "lifetime-call");
                if (MemoryRepresentationType(parameter.Type) is PointerType formal)
                {
                    if (MemoryRepresentationType(argument.Value.Type) is not PointerType actual || argument.Value.Type.Size != 8 || argument.Value.Type.Alignment != 8 ||
                        actual.Size != 8 || actual.Alignment != 8 || actual.IsRValueReference ||
                        !MemoryRepresentationType(actual.PointeeType).Equals(MemoryRepresentationType(formal.PointeeType)))
                        throw Limit("A generated lifetime pointer argument changes its selected record representation.", "lifetime-call");
                    ValidateAggregateQualifiers(argument.Value.Type);
                    ValidateAggregateQualifiers(actual.PointeeType);
                }
                else if (!argument.Value.Type.Equals(parameter.Type))
                    throw Limit("A generated lifetime scalar argument requires its exact retained type.", "lifetime-call");
            }
            var receiverIndex = signature.Parameters.ToList().FindIndex(parameter => parameter.Role.Name == "this_parameter");
            var resultIndex = signature.Parameters.ToList().FindIndex(parameter => parameter.Role.Name == "return_parameter");
            if ((receiverIndex < 0 ? call.Receiver is not null : call.Receiver is null || !call.Receiver.Equals(arguments[receiverIndex].Value)) ||
                (resultIndex < 0 ? call.Call.ReturnSlot is not null : call.Call.ReturnSlot is null || !call.Call.ReturnSlot.Equals(arguments[resultIndex].Value)))
                throw Limit("Receiver/result facets must alias the already retained execution argument; no argument is synthesized.", "lifetime-call");
        }

        private ReceiverMemberCleanup ValidateReceiverMemberCleanup(FunctionSymbol function, CleanupAction action,
            IReadOnlyList<Initialization> recipes, IReadOnlyList<SemanticNode> nodes, bool normalDestructor)
        {
            if (receiverMemberCleanups.TryGetValue(action, out var existing)) return existing;
            var body = function.Body!;
            if (action.Storage is not { Kind.Name: "receiver_relative", UsesThisParameterOffset: true, IsIndirect: false } location ||
                location.Variable is not null || location.Details.BaseEntity is not null || location.Offset < 0 ||
                !function.Signature.HasThisParameter || function.Parameters.Count == 0 || function.Parameters[0].Role.Name != "this_parameter" ||
                function.Parameters[0].Symbol is not { } receiver || function.SpecialFunctionKind?.Name != (normalDestructor ? "sfk_destructor" : "sfk_constructor") ||
                function.ConstructorOrDestructorKind?.Name != "cdk_complete" || action.TargetMethod is not { } destructor ||
                destructor.SpecialFunctionKind?.Name != "sfk_destructor" || destructor.Parameters.Count != 1)
                throw Limit("The member cleanup requires its selected complete constructor/destructor receiver and direct this-relative storage.", "lifetime-member");
            var whole = GeneratedRecordPointer(function.Parameters[0].Type, false);
            var member = GeneratedRecordPointer(destructor.Parameters[0].Type, false);
            if (whole.Layout is not { } layout || whole.Kind.Name is not ("tk_struct" or "tk_class") || whole.IsPacked || member.IsPacked || whole.Bases.Count != 0 ||
                whole.HasVirtualBases || whole.HasVirtualFunctions || whole.Size is null || member.Size is null || member.Alignment is null ||
                whole.Alignment is null || member.Alignment <= 0 || whole.Alignment < member.Alignment ||
                location.Offset + member.Size > whole.Size || location.Offset % member.Alignment != 0)
                throw Limit("The receiver-relative member needs a complete ordinary in-bounds captured layout.", "lifetime-member");
            var members = layout.Fields.Where(layoutField => layoutField.Offset == location.Offset && layoutField.BitWidth is null &&
                MemoryRepresentationType(layoutField.Field.Type).Equals(member)).ToArray();
            if (members.Length != 1)
                throw Limit("The member cleanup displacement must identify one exact captured nonbitfield record field.", "lifetime-member");
            var field = members[0].Field;
            ValidateAggregateQualifiers(field.Type);
            var matches = recipes.Where(candidate => candidate.Target is null && candidate.Details.Type is null && candidate.Destructor?.Equals(destructor) == true &&
                candidate.Details.IsConstructorInitialization && candidate.Lifetime?.BoundEntity?.Equals(body.Scope) == true).ToArray();
            if (matches.Length != 1)
                throw Limit("The this-relative member cleanup must have one scope-bound retained subobject destruction recipe.", "lifetime-member");
            var recipe = matches[0];
            ValidateExceptionMetadataRecipe(recipe, false, true);
            ValidateGeneratedLifetimeRoutine(destructor);
            EnsureGeneratedNoThrow(destructor);
            var calls = nodes.OfType<CallOperation>().Where(call => call.Receiver is { } address && IsAddressOfReceiverField(address, receiver, field)).ToArray();
            if (normalDestructor)
            {
                if (body.Scope?.ConstructorInitializers.Count(initializer => initializer.Kind.Name == "cik_field" && initializer.Field?.Equals(field) == true &&
                    initializer.Initialization?.Equals(recipe) == true) != 1 ||
                    calls.Count(call => call.TargetMethod?.Equals(destructor) == true) != 1)
                    throw Limit("The noexcept member destruction requires its retained field recipe and one explicit normal destructor call.", "lifetime-member");
            }
            else
            {
                if (body.Scope is null || body.Scope.ConstructorInitializers.Count != 0)
                    throw Limit("This constructor slice requires already-lowered member calls, without replaying initializer recipes.", "lifetime-member");
                var completed = calls.Where(call => call.TargetMethod is { } constructor && constructor.SpecialFunctionKind?.Name == "sfk_constructor" &&
                    constructor.ConstructorOrDestructorKind?.Name == "cdk_complete" && constructor.ContainingType?.Equals(member) == true &&
                    constructor.Signature.ExceptionSpecification?.NoexceptEffective == true && HasImmediateCompletionState(nodes, call, action.Details.Region)).ToArray();
                if (completed.Length != 1)
                    throw Limit("The member cleanup must be armed by the actual retained state immediately after its selected successful constructor call.", "lifetime-member");
                ValidateGeneratedLifetimeRoutine(completed[0].TargetMethod!);
                EnsureGeneratedNoThrow(completed[0].TargetMethod!);
            }
            var result = new ReceiverMemberCleanup(function, recipe, receiver, field, member);
            receiverMemberCleanups.Add(action, result);
            return result;
        }

        private static bool IsAddressOfReceiverField(ExpressionOperation expression, VariableSymbol receiver, FieldSymbol field) => expression is
            OperatorOperation { OperatorKind.Name: "eok_address_of" } address && address.Operands.Count == 1 &&
            address.Operands[0] is OperatorOperation { OperatorKind.Name: "eok_points_to_field" } selected && selected.Operands.Count == 2 &&
            selected.Operands[0] is ReferenceOperation { Target: VariableSymbol actualReceiver } && actualReceiver.Equals(receiver) &&
            selected.Operands[1] is ReferenceOperation { Target: FieldSymbol actualField } && actualField.Equals(field);

        private static bool HasImmediateCompletionState(IReadOnlyList<SemanticNode> nodes, CallOperation constructor, CleanupRegion region)
        {
            foreach (var block in nodes.OfType<BlockOperation>())
            {
                var statements = block.Statements;
                for (var index = 0; index + 1 < statements.Count; ++index)
                    if (statements[index].Expression?.Equals(constructor) == true &&
                        statements[index + 1].Expression is CleanupStateOperation { IsUnreachable: false } state &&
                        state.Region?.Equals(region) == true && state.StateNumber == region.Ordinal) return true;
            }
            return false;
        }

        private bool IsReceiverMemberRecipe(Initialization recipe) => receiverMemberCleanups.Values.Any(member => member.Recipe.Equals(recipe));
        private bool MatchesLifetimeCleanupRecipe(CleanupAction action, Initialization recipe) => receiverMemberCleanups.TryGetValue(action, out var member)
            ? member.Recipe.Equals(recipe)
            : action.Storage?.Variable?.Equals(recipe.Target) == true && action.TargetMethod?.Equals(recipe.Destructor) == true;

        private void ValidateNormalCleanupBody(FunctionSymbol function)
        {
            var body = function.Body!;
            var eh = body.ExceptionHandling;
            if (eh.CleanupRegions.Count == 0 && !body.Lifetimes.Any(lifetime => lifetime.Cleanups.Count != 0)) return;
            RequireNormalLifetimeProfile();
            EnsureGeneratedNoThrow(function);
            if (function.Signature.Routine.ExceptionSpecification?.NoexceptEffective != true || eh.Details is not { } details ||
                eh.RegionTable is not { } table || !body.Locals.Contains(table) || eh.ArrayTable is not null || eh.Arrays.Count != 0 ||
                details.InitialState != details.NullState || details.NullState != 65535 || details.Prologue is null || eh.Epilogues.Count == 0)
                throw Limit("Normal cleanup admission requires the exercised noexcept region-table representation and null state.", "lifetime");
            var nodes = NormalLifetimeNodes(body.Root).ToArray();
            if (nodes.OfType<ReferenceOperation>().Any(reference => reference.Target?.Equals(table) == true))
                throw Limit("The EH region table has a live typed value reference.", "lifetime");
            if (nodes.OfType<ConstantOperation>().Any(constant => constant.Constant.Value is not IntegerValue))
                throw Limit("The bounded normal cleanup body contains an unvalidated noninteger constant reference.", "lifetime");
            if (table.ExceptionTable is not { } projection || projection.Kind.Name != "cleanup" || !projection.Owner.Equals(table) ||
                !projection.FunctionEh.Routine.Equals(function) || projection.FunctionEh.RegionTable?.Equals(table) != true)
                throw Limit("The EH region table has no matching retained table-projection association.", "lifetime");
            var tableStatics = table.LocalStaticInitializations;
            if (table.Initializer is not null || table.ConstantInitializer is not null ||
                tableStatics.Any(initializer => !initializer.Variable.Equals(table) || initializer.Kind.Name != "initk_static" ||
                    initializer.Initialization is not null || initializer.Constant is not null || initializer.Lifetime is not null))
                throw Limit("The EH region table has an application initialization association.", "lifetime");
            var calls = nodes.OfType<CallOperation>().ToArray();
            foreach (var call in calls)
            {
                if (call.Callee is not ReferenceOperation { Target: FunctionSymbol target } || !functions.Contains(target) ||
                    call.TargetMethod is null || !call.TargetMethod.Equals(target) || target.Signature.IsVariadic ||
                    target.Signature.Routine.ExceptionSpecification?.NoexceptEffective != true)
                    throw Limit("The normal-only cleanup path requires retained direct noexcept generated callees.", "lifetime");
                ValidateGeneratedCallRoles(call);
            }
            var regions = eh.CleanupRegions;
            var lifetimes = body.Lifetimes;
            var recipes = lifetimes.SelectMany(lifetime => lifetime.Cleanups.Select(cleanup => (Lifetime: lifetime, Cleanup: cleanup))).ToArray();
            var actions = new List<CleanupAction>();
            for (var index = 0; index < regions.Count; ++index)
            {
                var region = regions[index];
                if (region.Ordinal != index || !region.Details.FunctionEh.Routine.Equals(function) || region.Condition is not null ||
                    region.Details.IsSupplement || region.Details.SupplementOwner is not null || region.Actions.Count != 1 ||
                    region.NextRegion is { } next && (!regions.Contains(next) || next.Ordinal >= region.Ordinal))
                    throw Limit("Only the retained simple normal destructor region chain is admitted.", "lifetime");
                var action = region.Actions[0];
                var flags = action.Details;
                if (action.Kind.Name != "destructor" || flags.Ordinal != 0 || !flags.Region.Equals(region) || action.Array is not null || action.AbiDetail is not null ||
                    flags.IsArray || flags.IsIndirect || flags.IsConditional || flags.IsBaseSubobject || flags.IsVla || flags.IsDeallocation || flags.IsLocalStaticGuard ||
                    action.Storage is not { } storage)
                    throw Limit("Normal cleanup metadata requires one validated local/member destructor without conditions, arrays or ABI side actions.", "lifetime");
                if (storage.Kind.Name == "receiver_relative")
                    ValidateReceiverMemberCleanup(function, action, recipes.Select(recipe => recipe.Cleanup.Initialization).ToArray(), nodes, true);
                else
                {
                    if (storage.Kind.Name != "direct_variable" || storage.Variable is not { } variable || !body.Locals.Contains(variable) ||
                        storage.Offset != 0 || storage.IsIndirect || storage.UsesThisParameterOffset || storage.Details.BaseEntity is not null ||
                        action.TargetMethod is not { } destructor || destructor.SpecialFunctionKind?.Name != "sfk_destructor" ||
                        destructor.Parameters.Count != 1 || !GeneratedRecordPointer(destructor.Parameters[0].Type, false).Equals(MemoryRepresentationType(variable.Type)))
                        throw Limit("The normal local cleanup storage has no exercised representation.", "lifetime");
                    ValidateGeneratedLifetimeRoutine(destructor);
                    if (!calls.Any(call => call.TargetMethod?.Equals(destructor) == true && call.Receiver is { } receiver && IsAddressOfLocal(receiver, variable)))
                        throw Limit("A normal destructor metadata action has no matching retained explicit call receiver.", "lifetime");
                }
                actions.Add(action);
            }
            foreach (var (lifetime, cleanup) in recipes)
            {
                var recipe = cleanup.Initialization;
                var flags = recipe.Details;
                if (recipe.Kind.Name != "dik_none" || recipe.Operation?.RepresentationRole.Name != "metadata" || recipe.Lifetime is null || !recipe.Lifetime.Equals(lifetime) ||
                    recipe.Constructor is not null || recipe.Expression is not null || recipe.Constant is not null || recipe.Operands.Count != 0 ||
                    recipe.Master is not null || recipe.OverlappingLifetime is not null || recipe.InitExpressionLifetime is not null ||
                    flags.IsConditional || flags.IsUnordered || flags.IsReusedValue || flags.IsOverlapping || flags.IsClassResultMaster || flags.IsStaticTemporary ||
                    flags.IsPartialAggregate || flags.IsPartialInitialization || flags.IsConstructorInitialization != IsReceiverMemberRecipe(recipe) || flags.FreesStorage || flags.FreesArray ||
                    flags.IsConditionalGuard || flags.IsVlaDeallocation || flags.IsFreeingOfExceptionObject || flags.CreatesInitializerList || flags.IsInitializerListArray ||
                    recipe.NextInDestructionList is { } next && !recipes.Any(other => other.Cleanup.Initialization.Equals(next)) ||
                    actions.Count(action => MatchesLifetimeCleanupRecipe(action, recipe)) != 1)
                    throw Limit("Lifetime cleanup recipes must only describe the already retained simple destructor actions.", "lifetime");
                if (lifetime.BoundEntity is Scope scope ? body.Scope?.Equals(scope) != true && !nodes.OfType<BlockOperation>().Any(block => block.Scope?.Equals(scope) == true)
                    : lifetime.BoundEntity is not LifetimeOperation wrapper || !nodes.Contains(wrapper) || wrapper.Lifetime?.Equals(lifetime) != true)
                    throw Limit("A reachable cleanup lifetime is not bound to an admitted body block or full-expression wrapper.", "lifetime");
            }
            if (actions.Count != recipes.Length || actions.Any(action => recipes.Count(recipe =>
                MatchesLifetimeCleanupRecipe(action, recipe.Cleanup.Initialization)) != 1))
                throw Limit("Cleanup actions and metadata destruction recipes do not have a unique matching association.", "lifetime");
            foreach (var marker in nodes.OfType<LoweredEhOperation>()) ValidateNormalCleanupMarker(marker, function, regions);
            if (!nodes.Contains(details.Prologue) || eh.Epilogues.Any(epilogue => !nodes.Contains(epilogue)))
                throw Limit("The retained normal EH prologue/epilogues are absent from the typed body occurrences.", "lifetime");
            foreach (var wrapper in nodes.OfType<LifetimeOperation>())
            {
                if (wrapper.Lifetime is not { } lifetime || !lifetimes.Contains(lifetime) || lifetime.BoundEntity?.Equals(wrapper) != true ||
                    lifetime.Kind.Name != "olk_expr_temporary" || wrapper.Operands.Count != 0 ||
                    wrapper.Value is not OperatorOperation { OperatorKind.Name: "eok_comma" } ||
                    !wrapper.Type.Equals(wrapper.Value.Type) || !wrapper.ValueCategory.Equals(wrapper.Value.ValueCategory))
                    throw Limit("The exercised normal full-expression wrapper requires its exact already-lowered comma value.", "lifetime");
            }
            // This only validates associations. Normal destructor calls remain in
            // their actual expression/statement positions; metadata is not replayed.
            normalCleanupFunctions.Add(function);
            omittedNormalRegionTables.Add(function, table);
            foreach (var marker in nodes.OfType<LoweredEhOperation>()) normalCleanupMarkers.Add(marker);
            foreach (var wrapper in nodes.OfType<LifetimeOperation>()) normalLifetimeExpressions.Add(wrapper);
        }

        private static bool IsAddressOfLocal(ExpressionOperation expression, VariableSymbol variable) => expression is
            OperatorOperation { OperatorKind.Name: "eok_address_of" } address && address.Operands.Count == 1 &&
            address.Operands[0] is ReferenceOperation { Target: VariableSymbol target } && target.Equals(variable);

        private static void ValidateNormalCleanupMarker(LoweredEhOperation marker, FunctionSymbol function, IReadOnlyList<CleanupRegion> regions)
        {
            var eh = function.Body!.ExceptionHandling;
            var payload = marker.ExceptionOperation;
            if (marker.RepresentationRole.Name != "execution" || marker.ValueCategory.Name != "prvalue" ||
                MemoryRepresentationType(marker.Type).Kind.Name != "tk_void" || marker.Operands.Count != 0 || marker.Details.VolatileFetch ||
                payload.FunctionEh?.Routine.Equals(function) != true || payload.Initialization is not null || payload.Handler is not null ||
                payload.TryBlock is not null || payload.TryExpression is not null || payload.CatchExpression is not null)
                throw Limit("A normal EH marker has additional retained execution obligations.", "lifetime");
            if (marker is CleanupStateOperation state)
            {
                if (marker.ExceptionKind.Name != (state.IsUnreachable ? "leck_unreachable_cleanup_state" : "leck_cleanup_state") ||
                    payload.Routine is not null || payload.StateNumber != state.StateNumber || !Equals(payload.Region, state.Region) ||
                    (state.Region is { } region ? !regions.Contains(region) || state.StateNumber != region.Ordinal : state.StateNumber != eh.Details!.NullState))
                    throw Limit("A normal cleanup-state marker does not select its actual region or retained null state.", "lifetime");
                return;
            }
            if (payload.Region is not null || payload.StateNumber is not null ||
                (marker.ExceptionKind.Name == "leck_function_prologue" ? !marker.Equals(eh.Details!.Prologue) || payload.Routine is not null
                    : marker.ExceptionKind.Name != "leck_function_epilogue" || !eh.Epilogues.Contains(marker) || payload.Routine?.Equals(function) != true))
                throw Limit("Only the actual normal function prologue/epilogue markers may be omitted.", "lifetime");
        }

        private bool IsOmittedNormalRegionTable(VariableSymbol variable) => currentFunction is { } function &&
            omittedNormalRegionTables.TryGetValue(function, out var table) && table.Equals(variable);

        private bool NeedsNormalLifetimePlan(ExpressionOperation expression) => expression is LoweredEhOperation or LifetimeOperation &&
            currentFunction is { } function && normalCleanupFunctions.Contains(function);

        private bool TryPlanNormalLifetime(ExpressionOperation expression, out ExpressionPlan plan)
        {
            plan = null!;
            if (currentFunction is not { } function || !normalCleanupFunctions.Contains(function)) return false;
            if (expression is LoweredEhOperation marker && normalCleanupMarkers.Contains(marker))
            {
                ValidateNormalCleanupMarker(marker, function, function.Body!.ExceptionHandling.CleanupRegions);
                plan = new(expression.Type, expression.ValueCategory, [], new CsLeaf(CsType.Void, "", true), ExpressionEffects.None);
                return true;
            }
            if (expression is LifetimeOperation wrapper && normalLifetimeExpressions.Contains(wrapper))
            {
                plan = PlanExpression(wrapper.Value) with { Type = wrapper.Type, ValueCategory = wrapper.ValueCategory };
                return true;
            }
            return false;
        }

        // Validation only, never an execution schedule. These are the exact typed
        // relationships consumed by the admitted normal body projection. Metadata
        // recipes, scope reachability and structural descendants add no evaluation.
        private static IEnumerable<SemanticNode> NormalLifetimeNodes(SemanticNode root)
        {
            return Walk(root, [], 0);
            static IEnumerable<SemanticNode> Walk(SemanticNode node, HashSet<SemanticNode> active, int depth)
            {
                if (depth > 256 || !active.Add(node)) throw Limit("The normal lifetime validation graph is cyclic or too deep.", "lifetime");
                try
                {
                    if (node.RepresentationRole.Name == "metadata") throw Limit("A normal execution occurrence unexpectedly names metadata.", "lifetime");
                    yield return node;
                    IEnumerable<SemanticNode> children = node switch
                    {
                        BlockOperation block => block.Statements,
                        ConditionalOperation conditional => new SemanticNode?[] { conditional.Condition, conditional.Then, conditional.Else }.OfType<SemanticNode>(),
                        JumpOperation { JumpKind.Name: "return" } jump => jump.Value is { } value ? [value] : [],
                        DeclarationOperation => [],
                        LifetimeOperation lifetime => [lifetime.Value],
                        ConditionOperation { Initialization: null, DynamicInitialization: null, Value: { } value } => [value],
                        CallOperation call => call.Operands,
                        LoweredEhOperation marker => marker.Operands,
                        ReferenceOperation or ConstantOperation => [],
                        OperatorOperation operation => operation.Operands,
                        StatementOperation { Kind.Name: "stmk_expr", Expression: { } expression } => [expression],
                        StatementOperation { Kind.Name: "stmk_empty" } => [],
                        _ => throw Limit($"Execution form '{node.Kind}' is outside the bounded normal-lifetime adapter.", "lifetime")
                    };
                    foreach (var child in children)
                        foreach (var descendant in Walk(child, active, depth + 1)) yield return descendant;
                }
                finally { active.Remove(node); }
            }
        }
    }
}
