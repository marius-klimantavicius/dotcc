namespace RoslynObj.CSharp;

// Selected public identities stay within the translator while their input is
// open. Generated projects and the runtime do not retain reader entities.
internal sealed record SemanticRuntimeFunctionImplementation(FunctionSymbol Function, string Contract)
{
    internal const string Int32ExceptionOwnerContract = "managed-cpp-int32-exception-owner-v1";
    internal const string RecordExceptionOwnerContract = "managed-cpp-exact-record-owner-v1";
    internal const string RecordConstructionOwnerContract = "managed-cpp-exact-record-construction-owner-v1";
    internal const string Int32RethrowOwnerContract = "managed-cpp-int32-rethrow-owner-v1";
    internal const string Int32NoexceptTerminationOwnerContract = "managed-cpp-int32-noexcept-unwind-true-failfast-owner-v1";
    internal const string GeneratedVectorConstructorContract = "generated-cpp-vector-constructor-v1";
    internal const string GeneratedVectorDestructorContract = "generated-cpp-vector-destructor-v1";
    internal const string SelectedStartupRegistrationContract = "generated-selected-program-atexit-v1";

    // A spelling permits deferral only. The emitter must return the actual
    // validated public function identity with the corresponding contract.
    internal static string? CandidateContract(string? linkageName) => linkageName switch
    {
        "__destroy_exception_object" => Int32ExceptionOwnerContract,
        "__cxa_vec_ctor" => GeneratedVectorConstructorContract,
        "__cxa_vec_dtor" => GeneratedVectorDestructorContract,
        "__cxa_atexit" => SelectedStartupRegistrationContract,
        _ => null
    };

    internal static bool IsAdmittedContract(string? linkageName, string contract) =>
        CandidateContract(linkageName) == contract ||
        linkageName == "__destroy_exception_object" &&
        contract is Int32RethrowOwnerContract or Int32NoexceptTerminationOwnerContract or RecordExceptionOwnerContract or RecordConstructionOwnerContract;

    internal ImportReport Describe(ImportReport import) => Contract switch
    {
        SelectedStartupRegistrationContract => import with
        {
            Provider = "RoslynObj.CSharp.Runtime.ManagedSelectedProgramLifetime",
            BindingStatus = "generated-selected-program-atexit-contract-selected",
            Ownership = "An explicit generated program instance retains exact selected managed destructor, borrowed object address and generated module address only after successful registration. Explicit shutdown invokes successful registrations once in reverse order; storage remains owned by generated program storage.",
            ErrorContract = "The actual registration result flows to the captured consumer; nonzero failure publishes no entry and creates no compensating destruction. Initial admitted startup and destructors have separate closed generated-body nonthrow proofs, not invented source noexcept. Unexpected managed faults latch a failed explicit lifecycle state and propagate directly, without guessed C++ cleanup or CLR application type-initializer wrapping.",
            ThreadContract = "Explicit serialized instance lifecycle; concurrent/reentrant transitions and registration during drain remain unsupported. No TLS, ProcessExit, native module unload or general __cxa_finalize contract is supplied.",
            CallbackContract = "Only the actual selected generated destructor body receives the retained object address through an ordinary managed thunk; no native function address, UCO callback or application fallback is supplied."
        },
        RecordConstructionOwnerContract => import with
        {
            Provider = "RoslynObj.CSharp.Runtime.ManagedCppException",
            BindingStatus = "generated-exact-record-construction-owner-contract-selected",
            Ownership = "The validated managed-cpp-exact-record-construction-owner-v1 contract owns captured complete 16-byte/8-aligned payload storage. The retained direct constructor operand receives the pending address; commit follows its successful completion. Failed construction follows active EDG cleanup before freeing pending storage without payload destruction. An eligible completed final release invokes the selected generated destructor once before freeing.",
            ErrorContract = "Only the admitted generated exact-type direct construction and const-reference catch participate. Constructor failure propagates a distinct selected owner. Copy-marker semantics, hierarchy matching, record rethrow/termination, native C++ unwinding and CLR-error conversion are not supplied by this contract.",
            ThreadContract = "Each explicit pending or completed carrier owns its payload on its creating managed thread; no ambient current owner or native exception stack is substituted.",
            CallbackContract = "One static ordinary managed destructor thunk invokes the actual selected generated body while completed payload storage is valid. No native entry, UCO address or native application implementation is supplied."
        },
        RecordExceptionOwnerContract => import with
        {
            Provider = "RoslynObj.CSharp.Runtime.ManagedCppException",
            BindingStatus = "generated-exact-record-owner-contract-selected",
            Ownership = "The validated managed-cpp-exact-record-owner-v1 contract owns one captured complete 16-byte/8-aligned nonpolymorphic payload. Its retained exception-start marker precedes the selected generated copy; commit follows successful initialization. Final eligible handler release invokes the exact selected generated destructor once before freeing; pending initialization release never invokes that destructor.",
            ErrorContract = "Only the admitted generated same-type record throw and const-reference catch participate. Class rethrow, throwing copy/destructor, base matching, class termination, native C++ unwinding and CLR-error conversion are unsupported.",
            ThreadContract = "The explicit carrier, payload and selected handler remain on their creating managed thread; no ambient current payload or native exception stack is substituted.",
            CallbackContract = "One static ordinary managed destructor thunk calls the actual selected generated body. No native entry, UCO function address or native application implementation is supplied."
        },
        Int32ExceptionOwnerContract => import with
        {
            Provider = "RoslynObj.CSharp.Runtime.ManagedCppException",
            BindingStatus = "generated-exception-owner-contract-selected",
            Ownership = "The validated managed-cpp-int32-exception-owner-v1 contract releases the selected handler's explicit payload owner once; no ambient current pointer or native application implementation is used.",
            ErrorContract = "Only the admitted generated exception carrier participates; native C++ unwinding and CLR-error conversion are unsupported.",
            ThreadContract = "The payload belongs to its carrier and selected handler; no ambient thread-local exception pointer is substituted.",
            CallbackContract = "No native entry or callback is supplied for this generated cleanup action."
        },
        Int32RethrowOwnerContract => import with
        {
            Provider = "RoslynObj.CSharp.Runtime.ManagedCppException",
            BindingStatus = "generated-rethrow-owner-contract-selected",
            Ownership = "The validated managed-cpp-int32-rethrow-owner-v1 contract retains one primary payload across two explicit propagation/handling entries. Each eligible handler entry releases once; only the final reference frees the original payload.",
            ErrorContract = "Only the admitted generated integer throw/rethrow carriers participate. Outer handler selection precedes inner cleanup; new throws from handlers, termination, native C++ unwinding and CLR-error conversion are unsupported.",
            ThreadContract = "The primary owner and propagation entries remain on their creating managed thread; no ambient current-handler registry or thread-local exception pointer is substituted.",
            CallbackContract = "No native entry or callback is supplied; actual handler-context tokens select the retained handling entry."
        },
        Int32NoexceptTerminationOwnerContract => import with
        {
            Provider = "RoslynObj.CSharp.Runtime.ManagedCppException",
            BindingStatus = "generated-noexcept-termination-owner-contract-selected",
            Ownership = "The validated managed-cpp-int32-noexcept-unwind-true-failfast-owner-v1 contract preserves explicit handler ownership for retained catch paths. On the selected noexcept termination path the process retains the payload; no caught transition or synthetic release occurs.",
            ErrorContract = "Selected runtime policy ManagedCppTerminationPolicy.UnwindBeforeTerminateDefaultFailFastV1: UNWIND_STACK_BEFORE_CALLING_TERMINATE=TRUE, default-only Environment.FailFast mapping on Linux/.NET, nonreturning SIGABRT. This is provider policy, not a captured capability. The distinct termination destination is selected before actual leaf/boundary cleanup; the caller handler is not selected. FailFast marker: RoslynObj.CSharp.ManagedCppException:default-noexcept-termination:unwind-true:failfast-v1. Custom terminate, program-uncaught policy, native C++ unwinding and CLR-error conversion are unsupported.",
            ThreadContract = "The payload and explicit boundary token remain on their creating managed thread; no ambient termination handler, thread-local current payload or cross-thread transfer is supplied.",
            CallbackContract = "No native entry, observer or custom termination callback is supplied. Default FailFast occurs after the captured cleanup chain reaches the selected noexcept boundary."
        },
        GeneratedVectorConstructorContract or GeneratedVectorDestructorContract => import with
        {
            Provider = "Generated.AutomaticArrayHelpers",
            BindingStatus = Contract == GeneratedVectorConstructorContract
                ? "generated-vector-constructor-contract-selected" : "generated-vector-destructor-contract-selected",
            Ownership = Contract == GeneratedVectorConstructorContract
                ? $"The validated {Contract} contract borrows automatic storage and advances completed count only after the selected constructor returns; failure destroys only the completed prefix in reverse. No allocation, cookie management or free occurs."
                : $"The validated {Contract} contract borrows automatic storage and invokes the selected no-throw destructor in reverse for the validated element range. No allocation, cookie management or free occurs.",
            ErrorContract = "Only the admitted generated exception carrier participates. Handler search precedes completed-prefix cleanup; native C++ unwinding, destructor failure and CLR-error conversion are unsupported.",
            ThreadContract = "Completion state is local to the synchronous generated helper invocation; no ambient vector-construction stack or thread-local count is substituted.",
            CallbackContract = "Captured constructor/destructor identities become direct managed calls inside the validated helper context; no native UCO entry, callback address or native application implementation is supplied."
        },
        _ => throw new InvalidOperationException("An unvalidated semantic runtime contract reached report generation.")
    };
}
