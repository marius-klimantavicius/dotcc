using Managed.Emulation.Host;

namespace Managed.Emulation.Execution;

// The observer installed by ThreadedGuestExecution drains THIS Authority's
// actual native enrollment. No borrowed lease, registration or new owner state.
internal sealed unsafe partial class FullTranslationJit : IHostMemoryFileNativeReadOwner
{
    object IHostMemoryFileNativeReadOwner.MemoryFileNativeReadAuthority => Authority;
}
