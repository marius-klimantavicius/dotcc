using System;
using System.Threading;
using Shouldly;
using Xunit;
using RuntimeLibc = DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class LibcHeapOwnershipTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Disposal_reclaims_only_its_owner_and_uses_each_recorded_allocator(bool debug)
    {
        bool previous = RuntimeLibc._dbgHeap;
        using var first = new RuntimeLibc.RuntimeContext();
        using var second = new RuntimeLibc.RuntimeContext();
        RuntimeLibc._dbgHeap = debug;
        try
        {
            using (first.Enter())
            {
                ((nint)RuntimeLibc.malloc(13)).ShouldNotBe(0);
                byte* zeros = (byte*)RuntimeLibc.calloc(4, 7);
                ((nint)zeros).ShouldNotBe(0);
                for (int i = 0; i < 28; ++i) zeros[i].ShouldBe((byte)0);
                void* aligned = null;
                RuntimeLibc.posix_memalign(&aligned, 256, 19).ShouldBe(0);
                ((nuint)aligned % 256).ShouldBe((nuint)0);
                RuntimeLibc.memset(aligned, 0xab, 19);
            }
            first.NativeAllocations.Count.ShouldBe(3);
            using (second.Enter())
            {
                byte* survivor = (byte*)RuntimeLibc.malloc(4);
                survivor[0] = 42;
                // Even switching the debug mode cannot change the allocator
                // used to release any already-owned plain/debug/aligned block.
                RuntimeLibc._dbgHeap = !debug;
                first.Dispose();
                first.NativeAllocations.ShouldBeEmpty();
                second.NativeAllocations.Count.ShouldBe(1);
                survivor[0].ShouldBe((byte)42);
                RuntimeLibc.free(survivor);
                second.NativeAllocations.ShouldBeEmpty();
            }
            first.Dispose();
        }
        finally { RuntimeLibc._dbgHeap = previous; }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Realloc_failure_retains_old_bytes_and_success_updates_ownership(bool debug, bool aligned)
    {
        bool previous = RuntimeLibc._dbgHeap;
        RuntimeLibc._dbgHeap = debug;
        using var owner = new RuntimeLibc.RuntimeContext();
        try
        {
            using (owner.Enter())
            {
                void* original;
                if (aligned) RuntimeLibc.posix_memalign(&original, 64, 16).ShouldBe(0);
                else original = RuntimeLibc.malloc(16);
                ((nint)original).ShouldNotBe(0);
                RuntimeLibc.memset(original, 0x6d, 16);
                ((nint)RuntimeLibc.realloc(original, -128)).ShouldBe(0);
                owner.NativeAllocations.Count.ShouldBe(1);
                for (int i = 0; i < 16; ++i) ((byte*)original)[i].ShouldBe((byte)0x6d);
                byte* grown = (byte*)RuntimeLibc.realloc(original, 128);
                ((nint)grown).ShouldNotBe(0);
                owner.NativeAllocations.Count.ShouldBe(1);
                owner.NativeAllocations.ShouldContain((nuint)grown);
                for (int i = 0; i < 16; ++i) grown[i].ShouldBe((byte)0x6d);
                grown[127] = 81;
                byte* shrunk = (byte*)RuntimeLibc.realloc(grown, 8);
                ((nint)shrunk).ShouldNotBe(0);
                for (int i = 0; i < 8; ++i) shrunk[i].ShouldBe((byte)0x6d);
                RuntimeLibc.free(shrunk);
                owner.NativeAllocations.ShouldBeEmpty();
            }
        }
        finally { RuntimeLibc._dbgHeap = previous; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Foreign_live_pointers_are_rejected_before_access(bool aligned)
    {
        using var first = new RuntimeLibc.RuntimeContext();
        using var second = new RuntimeLibc.RuntimeContext();
        void* pointer;
        using (first.Enter())
        {
            if (aligned) RuntimeLibc.posix_memalign(&pointer, 64, 16).ShouldBe(0);
            else pointer = RuntimeLibc.malloc(16);
            ((byte*)pointer)[0] = 61;
        }
        nint address = (nint)pointer;
        Should.Throw<InvalidOperationException>(() => RuntimeLibc.free((void*)address)).Message.ShouldContain("different C runtime context");
        Should.Throw<InvalidOperationException>(() => RuntimeLibc.realloc((void*)address, 32)).Message.ShouldContain("different C runtime context");
        using (second.Enter())
        {
            Should.Throw<InvalidOperationException>(() => RuntimeLibc.free((void*)address));
            Should.Throw<InvalidOperationException>(() => RuntimeLibc.realloc((void*)address, 32));
            second.NativeAllocations.ShouldBeEmpty();
        }
        using (first.Enter())
        {
            ((byte*)pointer)[0].ShouldBe((byte)61);
            RuntimeLibc.free(pointer);
            Should.Throw<InvalidOperationException>(() => RuntimeLibc.free((void*)address));
            Should.Throw<InvalidOperationException>(() => RuntimeLibc.realloc((void*)address, 32));
        }
    }

    [Fact]
    public void Legacy_allocation_can_only_be_freed_outside_an_owned_context()
    {
        nint legacy = (nint)RuntimeLibc.malloc(16);
        using var owner = new RuntimeLibc.RuntimeContext();
        try
        {
            using (owner.Enter())
            {
                Should.Throw<InvalidOperationException>(() => RuntimeLibc.free((void*)legacy)).Message.ShouldContain("foreign or already released");
                Should.Throw<InvalidOperationException>(() => RuntimeLibc.realloc((void*)legacy, 32));
                RuntimeLibc.free(null);
                owner.NativeAllocations.ShouldBeEmpty();
            }
            byte* resized = (byte*)RuntimeLibc.realloc((void*)legacy, 32);
            ((nint)resized).ShouldNotBe(0);
            legacy = (nint)resized;
            resized[31] = 7;
        }
        finally { RuntimeLibc.free((void*)legacy); }
    }

    [Fact]
    public void String_and_allocating_path_results_share_the_owner_free_contract()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using (owner.Enter())
        {
            byte* source = stackalloc byte[] { (byte)'a', (byte)'b', 0 };
            byte* copy = RuntimeLibc.strdup(source);
            byte* prefix = RuntimeLibc.strndup(source, 1);
            byte* cwd = RuntimeLibc.getcwd(null, 0);
            byte* full = RuntimeLibc.realpath(cwd, null);
            owner.NativeAllocations.Count.ShouldBe(4);
            copy[1].ShouldBe((byte)'b'); prefix[1].ShouldBe((byte)0);
            RuntimeLibc.strcmp(cwd, full).ShouldBe(0);
            RuntimeLibc.free(copy); RuntimeLibc.free(prefix); RuntimeLibc.free(cwd); RuntimeLibc.free(full);
            owner.NativeAllocations.ShouldBeEmpty();
        }
    }

    [Fact]
    public void Failed_and_zero_size_requests_keep_precise_ownership()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using (owner.Enter())
        {
            ((nint)RuntimeLibc.malloc(-128)).ShouldBe(0);
            ((nint)RuntimeLibc.calloc(-1, -1)).ShouldBe(0);
            ((nint)RuntimeLibc.realloc(null, -128)).ShouldBe(0);
            void* output = (void*)1234;
            RuntimeLibc.errno = RuntimeLibc.ERANGE;
            RuntimeLibc.posix_memalign(&output, 7, 4).ShouldBe(RuntimeLibc.EINVAL);
            RuntimeLibc.posix_memalign(&output, 64, ulong.MaxValue).ShouldBe(RuntimeLibc.ENOMEM);
            ((nint)output).ShouldBe((nint)1234);
            RuntimeLibc.errno.ShouldBe(RuntimeLibc.ERANGE);
            owner.NativeAllocations.ShouldBeEmpty();
            void* empty = RuntimeLibc.realloc(null, 0);
            if (empty != null) RuntimeLibc.free(empty);
            void* allocated = RuntimeLibc.realloc(null, 4);
            owner.NativeAllocations.Count.ShouldBe(1);
            ((nint)RuntimeLibc.realloc(allocated, 0)).ShouldBe(0);
            owner.NativeAllocations.ShouldBeEmpty();
        }
    }

    [Fact]
    public void Active_bindings_and_callback_leases_prevent_bulk_free()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using (owner.Enter())
        {
            byte* pointer = (byte*)RuntimeLibc.malloc(4);
            pointer[0] = 17;
            Should.Throw<InvalidOperationException>(owner.Dispose);
            owner.NativeAllocations.Count.ShouldBe(1);
            pointer[0].ShouldBe((byte)17);
        }
        using (owner.RetainLease()) Should.Throw<InvalidOperationException>(owner.Dispose);
        owner.NativeAllocations.Count.ShouldBe(1);
        owner.Dispose();
        owner.NativeAllocations.ShouldBeEmpty();
    }

    private static void* AllocateOnPthread(void* ignored)
    {
        byte* result = (byte*)RuntimeLibc.malloc(16);
        if (result != null) result[0] = 42;
        return result;
    }

    [Fact]
    public void Pthread_callbacks_allocate_in_the_captured_owner()
    {
        using var owner = new RuntimeLibc.RuntimeContext();
        using (owner.Enter())
        {
            long worker;
            RuntimeLibc.pthread_create(&worker, null, &AllocateOnPthread, null).ShouldBe(0);
            void* result;
            RuntimeLibc.pthread_join(worker, &result).ShouldBe(0);
            ((nint)result).ShouldNotBe(0);
            ((byte*)result)[0].ShouldBe((byte)42);
            owner.NativeAllocations.ShouldContain((nuint)result);
        }
        owner.Dispose();
        owner.NativeAllocations.ShouldBeEmpty();
    }

    [Fact]
    public void Concurrent_threads_share_only_their_explicit_owner_heap()
    {
        using var first = new RuntimeLibc.RuntimeContext();
        using var second = new RuntimeLibc.RuntimeContext();
        var owners = new[] { first, second, first, second };
        var errors = new Exception?[4];
        var threads = new Thread[4];
        for (int i = 0; i < threads.Length; ++i)
        {
            int slot = i;
            threads[i] = new Thread(() =>
            {
                try
                {
                    using var binding = owners[slot].Enter();
                    for (int j = 0; j < 100; ++j)
                    {
                        byte* pointer = (byte*)RuntimeLibc.malloc(16);
                        pointer[0] = (byte)slot;
                        byte* resized = (byte*)RuntimeLibc.realloc(pointer, 32);
                        resized[0].ShouldBe((byte)slot);
                        if (j % 2 == 0) RuntimeLibc.free(resized);
                    }
                }
                catch (Exception error) { errors[slot] = error; }
            });
            threads[i].Start();
        }
        foreach (var thread in threads) thread.Join(10000).ShouldBeTrue();
        foreach (var error in errors) error.ShouldBeNull();
        first.NativeAllocations.Count.ShouldBe(100);
        second.NativeAllocations.Count.ShouldBe(100);
        first.Dispose();
        first.NativeAllocations.ShouldBeEmpty();
        second.NativeAllocations.Count.ShouldBe(100);
        second.Dispose();
        second.NativeAllocations.ShouldBeEmpty();
    }
}
