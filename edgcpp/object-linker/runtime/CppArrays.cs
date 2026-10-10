#nullable enable
namespace Edg.Runtime;

// Portable Itanium array operations. Mirrors lib_src/vec_newdel.c: only
// completed elements are rolled back, in reverse construction order.
public static unsafe class CppArrays
{
    public static void Construct(void* data, ulong count, ulong stride,
        delegate* managed<void*, void> constructor, delegate* managed<void*, void> destructor)
    {
        ulong completed = 0;
        try
        {
            if (constructor != null)
                for (; completed < count; ++completed) constructor((byte*)data + completed * stride);
        }
        catch (global::System.Exception)
        {
            Cleanup(data, completed, stride, destructor);
            throw;
        }
    }

    public static void CopyConstruct(void* data, void* source, ulong count, ulong stride,
        delegate* managed<void*, void*, void> constructor, delegate* managed<void*, void> destructor)
    {
        ulong completed = 0;
        try
        {
            if (constructor != null)
                for (; completed < count; ++completed)
                    constructor((byte*)data + completed * stride, (byte*)source + completed * stride);
        }
        catch (global::System.Exception)
        {
            Cleanup(data, completed, stride, destructor);
            throw;
        }
    }

    public static void Destroy(void* data, ulong count, ulong stride, delegate* managed<void*, void> destructor)
    {
        if (data == null || destructor == null) return;
        try
        {
            while (count != 0) destructor((byte*)data + --count * stride);
        }
        catch (global::System.Exception)
        {
            Cleanup(data, count, stride, destructor);
            throw;
        }
    }

    public static void Cleanup(void* data, ulong count, ulong stride, delegate* managed<void*, void> destructor)
    {
        if (data == null || destructor == null) return;
        global::System.Exception? transfer = null;
        while (count != 0)
        {
            try { destructor((byte*)data + --count * stride); }
            catch (global::System.Exception error) when (CleanupFailure(error)) { transfer = error; }
        }
        if (transfer != null) throw transfer;
    }

    private static bool CleanupFailure(global::System.Exception error)
    {
        if (!ObjectProgram.IsRuntimeTransfer(error))
            global::System.Environment.FailFast("Array destructor threw during unwinding", error);
        return true;
    }
}
