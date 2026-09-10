using Shouldly;
using Xunit;
using RuntimeLibc = DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class LibcAllocationFailureTests
{
    // Converting -128 to size_t requests almost the complete address space.
    // This is rejected without exhausting host memory. Unlike -1, it also
    // avoids wrapping the OLD debug allocator's 64-byte overhead in red runs.
    private const int Impossible = -128;

    [Theory]
    [InlineData(-1)]
    [InlineData(-32)]
    [InlineData(-64)]
    public void Debug_heap_rejects_overhead_wrap_without_touching_memory(int size)
    {
        // Added after the safe red: the old allocator wraps these requests into
        // tiny blocks and then writes beyond them, so running them before the
        // guard would corrupt the test host instead of reporting an assertion.
        ((nint)RuntimeLibc.DbgAlloc((nuint)size, false)).ShouldBe(0);
    }

    [Fact]
    public void Debug_calloc_and_successful_realloc_preserve_zeroed_contents()
    {
        bool previous = RuntimeLibc._dbgHeap;
        RuntimeLibc._dbgHeap = true;
        byte* pointer = null;
        try
        {
            pointer = (byte*)RuntimeLibc.calloc(4, 4);
            ((nint)pointer).ShouldNotBe(0);
            for (int i = 0; i < 16; i++) pointer[i].ShouldBe((byte)0);
            pointer[15] = 42;
            byte* replacement = (byte*)RuntimeLibc.realloc(pointer, 32);
            ((nint)replacement).ShouldNotBe(0);
            pointer = replacement;
            pointer[15].ShouldBe((byte)42);
        }
        finally
        {
            RuntimeLibc.free(pointer);
            RuntimeLibc._dbgHeap = previous;
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Impossible_malloc_returns_null(bool debug)
    {
        bool previous = RuntimeLibc._dbgHeap;
        RuntimeLibc._dbgHeap = debug;
        try
        {
            void* pointer = RuntimeLibc.malloc(Impossible);
            try { ((nint)pointer).ShouldBe(0); }
            finally { RuntimeLibc.free(pointer); }
        }
        finally { RuntimeLibc._dbgHeap = previous; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Impossible_calloc_returns_null(bool debug)
    {
        bool previous = RuntimeLibc._dbgHeap;
        RuntimeLibc._dbgHeap = debug;
        try
        {
            void* pointer = RuntimeLibc.calloc(Impossible, 1);
            try { ((nint)pointer).ShouldBe(0); }
            finally { RuntimeLibc.free(pointer); }
        }
        finally { RuntimeLibc._dbgHeap = previous; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Calloc_product_overflow_returns_null(bool debug)
    {
        bool previous = RuntimeLibc._dbgHeap;
        RuntimeLibc._dbgHeap = debug;
        try
        {
            // The old debug path wraps this product to one byte, which is
            // safely freed below; the corrected allocator must reject it.
            void* pointer = RuntimeLibc.calloc(-1, -1);
            try { ((nint)pointer).ShouldBe(0); }
            finally { RuntimeLibc.free(pointer); }
        }
        finally { RuntimeLibc._dbgHeap = previous; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Failed_realloc_preserves_original_allocation(bool debug)
    {
        bool previous = RuntimeLibc._dbgHeap;
        RuntimeLibc._dbgHeap = debug;
        byte* original = null;
        try
        {
            original = (byte*)RuntimeLibc.malloc(16);
            ((nint)original).ShouldNotBe(0);
            for (int i = 0; i < 16; i++) original[i] = (byte)(i + 17);
            void* replacement = RuntimeLibc.realloc(original, Impossible);
            ((nint)replacement).ShouldBe(0);
            for (int i = 0; i < 16; i++) original[i].ShouldBe((byte)(i + 17));
        }
        finally
        {
            RuntimeLibc.free(original);
            RuntimeLibc._dbgHeap = previous;
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Failed_realloc_of_null_returns_null(bool debug)
    {
        bool previous = RuntimeLibc._dbgHeap;
        RuntimeLibc._dbgHeap = debug;
        try
        {
            void* pointer = RuntimeLibc.realloc(null, Impossible);
            try { ((nint)pointer).ShouldBe(0); }
            finally { RuntimeLibc.free(pointer); }
        }
        finally { RuntimeLibc._dbgHeap = previous; }
    }
}
