using System;
using System.IO;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using RuntimeLibc = DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class LibcPosixMemalignTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Valid_alignments_and_odd_sizes_are_writable_and_freeable(bool debug)
    {
        bool previous = RuntimeLibc._dbgHeap;
        RuntimeLibc._dbgHeap = debug;
        try
        {
            foreach (ulong alignment in new ulong[] { 8, 16, 32, 64, 256, 4096, 65536, 1048576 })
            foreach (ulong size in new ulong[] { 0, 1, 13, 127, 8193 })
            {
                void* pointer = null;
                RuntimeLibc.errno = RuntimeLibc.EDOM;
                RuntimeLibc.posix_memalign(&pointer, alignment, size).ShouldBe(0);
                try
                {
                    ((nuint)pointer).ShouldNotBe((nuint)0);
                    ((ulong)(nuint)pointer % alignment).ShouldBe(0UL);
                    RuntimeLibc.errno.ShouldBe(RuntimeLibc.EDOM);
                    for (ulong i = 0; i < size; i++) ((byte*)pointer)[i] = (byte)i;
                    for (ulong i = 0; i < size; i++) ((byte*)pointer)[i].ShouldBe((byte)i);
                }
                finally { RuntimeLibc.free(pointer); }
            }
            // Ordinary pointers remain valid while the registry is populated.
            void* aligned = null;
            RuntimeLibc.posix_memalign(&aligned, 64, 16).ShouldBe(0);
            try
            {
                void* ordinary = RuntimeLibc.malloc(19);
                RuntimeLibc.free(ordinary);
                RuntimeLibc.free(null);
            }
            finally { RuntimeLibc.free(aligned); }
        }
        finally { RuntimeLibc._dbgHeap = previous; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Errors_preserve_output_and_errno_without_narrowing_size_t(bool debug)
    {
        bool previous = RuntimeLibc._dbgHeap;
        RuntimeLibc._dbgHeap = debug;
        try
        {
            foreach (ulong alignment in new ulong[] { 0, 1, 2, 4, 7, 12, 24, 65, 0x100000008UL })
            {
                void* pointer = (void*)1234;
                RuntimeLibc.errno = RuntimeLibc.ERANGE;
                RuntimeLibc.posix_memalign(&pointer, alignment, 10).ShouldBe(RuntimeLibc.EINVAL);
                ((nuint)pointer).ShouldBe((nuint)1234);
                RuntimeLibc.errno.ShouldBe(RuntimeLibc.ERANGE);
            }
            foreach (ulong size in new ulong[] { ulong.MaxValue, ulong.MaxValue - 127, 0x7fffffffffffffffUL })
            {
                void* pointer = (void*)1234;
                RuntimeLibc.errno = RuntimeLibc.ERANGE;
                RuntimeLibc.posix_memalign(&pointer, 256, size).ShouldBe(RuntimeLibc.ENOMEM);
                ((nuint)pointer).ShouldBe((nuint)1234);
                RuntimeLibc.errno.ShouldBe(RuntimeLibc.ERANGE);
            }
            void* huge = (void*)1234;
            RuntimeLibc.posix_memalign(&huge, 1UL << 63, 1UL << 63).ShouldBe(RuntimeLibc.ENOMEM);
            ((nuint)huge).ShouldBe((nuint)1234);
        }
        finally { RuntimeLibc._dbgHeap = previous; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Realloc_preserves_contents_and_retains_original_on_failure(bool debug)
    {
        bool previous = RuntimeLibc._dbgHeap;
        RuntimeLibc._dbgHeap = debug;
        void* pointer = null;
        try
        {
            RuntimeLibc.posix_memalign(&pointer, 4096, 17).ShouldBe(0);
            for (int i = 0; i < 17; i++) ((byte*)pointer)[i] = (byte)(i + 33);
            ((nuint)RuntimeLibc.realloc(pointer, -128)).ShouldBe((nuint)0);
            for (int i = 0; i < 17; i++) ((byte*)pointer)[i].ShouldBe((byte)(i + 33));
            void* replacement = RuntimeLibc.realloc(pointer, 43);
            ((nuint)replacement).ShouldNotBe((nuint)0);
            pointer = replacement;
            for (int i = 0; i < 17; i++) ((byte*)pointer)[i].ShouldBe((byte)(i + 33));
        }
        finally
        {
            RuntimeLibc.free(pointer);
            RuntimeLibc._dbgHeap = previous;
        }
    }

    [Fact]
    public void Concurrent_allocations_can_be_freed_on_other_threads()
    {
        var pointers = new nuint[128];
        Parallel.For(0, pointers.Length, i =>
        {
            void* pointer = null;
            RuntimeLibc.posix_memalign(&pointer, 256, 37).ShouldBe(0);
            ((byte*)pointer)[36] = (byte)i;
            pointers[i] = (nuint)pointer;
        });
        Parallel.For(0, pointers.Length, i =>
        {
            try { ((byte*)pointers[i])[36].ShouldBe((byte)i); }
            finally { RuntimeLibc.free((void*)pointers[i]); }
        });
    }

    [Fact]
    public void Debug_heap_checks_the_requested_boundary_even_with_alignment_padding()
    {
        bool previous = RuntimeLibc._dbgHeap;
        TextWriter previousError = Console.Error;
        using var errors = new StringWriter();
        RuntimeLibc._dbgHeap = true;
        void* pointer = null;
        try
        {
            Console.SetError(errors);
            RuntimeLibc.posix_memalign(&pointer, 4096, 13).ShouldBe(0);
            ((byte*)pointer)[13] = 0;
            RuntimeLibc.free(pointer);
            pointer = null;
            errors.ToString().ShouldContain("write past end of 13-byte block");
        }
        finally
        {
            RuntimeLibc.free(pointer);
            Console.SetError(previousError);
            RuntimeLibc._dbgHeap = previous;
        }
    }
}
