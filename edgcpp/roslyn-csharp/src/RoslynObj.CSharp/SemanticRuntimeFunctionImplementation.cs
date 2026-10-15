namespace RoslynObj.CSharp;

// Selected public identities stay within the translator while their input is
// open. Generated projects and the runtime do not retain reader entities.
internal sealed record SemanticRuntimeFunctionImplementation(FunctionSymbol Function, string Contract)
{
    internal const string Int32ExceptionOwnerContract = "managed-cpp-int32-exception-owner-v1";
    internal const string Int32RethrowOwnerContract = "managed-cpp-int32-rethrow-owner-v1";
    internal const string Int32NoexceptTerminationOwnerContract = "managed-cpp-int32-noexcept-unwind-true-failfast-owner-v1";
    internal const string GeneratedVectorConstructorContract = "generated-cpp-vector-constructor-v1";
    internal const string GeneratedVectorDestructorContract = "generated-cpp-vector-destructor-v1";

    // A spelling permits deferral only. The emitter must return the actual
    // validated public function identity with the corresponding contract.
    internal static string? CandidateContract(string? linkageName) => linkageName switch
    {
        "__destroy_exception_object" => Int32ExceptionOwnerContract,
        "__cxa_vec_ctor" => GeneratedVectorConstructorContract,
        "__cxa_vec_dtor" => GeneratedVectorDestructorContract,
        _ => null
    };

    internal static bool IsAdmittedContract(string? linkageName, string contract) =>
        CandidateContract(linkageName) == contract ||
        linkageName == "__destroy_exception_object" &&
        contract is Int32RethrowOwnerContract or Int32NoexceptTerminationOwnerContract;

    internal ImportReport Describe(ImportReport import) => Contract switch
    {
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
