using System;
using System.IO;
using System.Runtime.InteropServices;
using DotCC;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

public sealed unsafe class UnmanagedJumpBufferTests
{
    [Fact]
    public void Heap_slot_keeps_numeric_identity_across_full_gc_and_normalizes_zero()
    {
        ulong* slot = (ulong*)NativeMemory.Alloc((nuint)sizeof(ulong));
        try
        {
            ulong identity = ArmJumpBuffer(slot);
            identity.ShouldNotBe(0UL);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var jump = Should.Throw<JumpBufferException>(() => longjmp(slot, 0));
            jump.Identity.ShouldBe(identity);
            jump.Value.ShouldBe(1);
            (*slot).ShouldBe(identity);
        }
        finally { NativeMemory.Free(slot); }
    }

    [Fact]
    public void Distinct_slots_and_rearming_the_same_slot_have_distinct_identities()
    {
        ulong* slots = stackalloc ulong[2];
        ulong outer = ArmJumpBuffer(slots);
        ulong inner = ArmJumpBuffer(slots + 1);
        ulong replacement = ArmJumpBuffer(slots);
        outer.ShouldNotBe(inner);
        replacement.ShouldNotBe(outer);
        replacement.ShouldNotBe(inner);
        Should.Throw<JumpBufferException>(() => longjmp(slots, 7)).Identity.ShouldBe(replacement);
        Should.Throw<JumpBufferException>(() => longjmp(slots + 1, 3)).Identity.ShouldBe(inner);
    }

    [Fact]
    public void Direct_unlowered_setjmp_and_unarmed_longjmp_fail_explicitly()
    {
        ulong slot = 0;
        // Pointer locals cannot be captured after taking a captured variable's
        // address, so pass the pointer value to the assertion lambdas.
        ulong* pointer = &slot;
        Should.Throw<InvalidOperationException>(() => setjmp(pointer));
        Should.Throw<InvalidOperationException>(() => longjmp(pointer, 1));
    }

    [Fact]
    public void Heap_aggregate_emission_arms_once_and_matches_captured_identity()
    {
        string path = Path.Combine(Path.GetTempPath(), $"dotcc-jump-slot-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, """
            #include <setjmp.h>
            #include <stdlib.h>
            struct State { jmp_buf halt; };
            static struct State *state;
            static struct State *Select(void) { return state; }
            int main(void) {
                state = malloc(sizeof(*state));
                if (setjmp(Select()->halt) == 0) longjmp(state->halt, 1);
                free(state);
                return 0;
            }
            """);
        try
        {
            string emitted = Compiler.EmitCSharp(new[] { path });
            emitted.ShouldContain("Libc.ArmJumpBuffer(Select()->halt)");
            emitted.ShouldContain("when (__jmp.Identity == __jmpIdentity");
            emitted.ShouldNotContain("when (__jmp.Identity == Select()");
            emitted.ShouldNotContain("halt = new Libc.LongJmpToken");
        }
        finally { File.Delete(path); }
    }
}
