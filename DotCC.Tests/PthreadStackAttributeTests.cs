using System.Runtime.CompilerServices;
using DotCC.Libc;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class PthreadStackAttributeTests
{
    private sealed class Owner : IProgramInstance, IDisposable
    {
        public RuntimeContext __DotCcRuntime { get; } = new();
        public void Dispose() => __DotCcRuntime.Dispose();
    }

    [Fact]
    public void Stack_attributes_validate_and_preserve_previous_value_on_error()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        int attribute = 0, detach = -1;
        ulong size = 0;
        pthread_attr_init(&attribute).ShouldBe(0);
        pthread_attr_getstacksize(&attribute, &size).ShouldBe(0);
        size.ShouldBe((ulong)DOTCC_PTHREAD_DEFAULT_STACK_SIZE);
        pthread_attr_setstacksize(&attribute, 4 * 1024 * 1024).ShouldBe(0);
        pthread_attr_setdetachstate(&attribute, PTHREAD_CREATE_DETACHED).ShouldBe(0);
        foreach (ulong invalid in new ulong[] { 0, PTHREAD_STACK_MIN - 1, (ulong)int.MaxValue + 1, ulong.MaxValue })
            pthread_attr_setstacksize(&attribute, invalid).ShouldBe(EINVAL);
        pthread_attr_getstacksize(&attribute, &size).ShouldBe(0);
        size.ShouldBe(4UL * 1024 * 1024);
        pthread_attr_getdetachstate(&attribute, &detach).ShouldBe(0);
        detach.ShouldBe(PTHREAD_CREATE_DETACHED);
        pthread_attr_getstacksize(&attribute, null).ShouldBe(EINVAL);
        pthread_attr_destroy(&attribute).ShouldBe(0);
        pthread_attr_getstacksize(&attribute, &size).ShouldBe(EINVAL);
        pthread_attr_destroy(&attribute).ShouldBe(EINVAL);
    }

    [Fact]
    public void Handles_are_owner_scoped_but_explicit_create_uses_its_owner()
    {
        using var first = new Owner(); using var second = new Owner();
        int firstAttribute = 0, secondAttribute = 0;
        using (first.__DotCcRuntime.Enter()) pthread_attr_init(&firstAttribute).ShouldBe(0);
        using (second.__DotCcRuntime.Enter())
        {
            pthread_attr_init(&secondAttribute).ShouldBe(0);
            firstAttribute.ShouldNotBe(secondAttribute);
            ulong size = 0;
            pthread_attr_getstacksize(&firstAttribute, &size).ShouldBe(EINVAL);
            pthread_attr_destroy(&firstAttribute).ShouldBe(EINVAL);
            long thread = 0;
            pthread_create(first, &thread, &firstAttribute, &OwnedWorker, null).ShouldBe(0);
            using (first.__DotCcRuntime.Enter())
            {
                void* result = null;
                pthread_join(thread, &result).ShouldBe(0);
                ((nint)result).ShouldBe((nint)71);
                pthread_attr_destroy(&firstAttribute).ShouldBe(0);
            }
            pthread_attr_destroy(&secondAttribute).ShouldBe(0);
        }
    }

    private static void* OwnedWorker(Owner owner, void* argument) =>
        ReferenceEquals(RuntimeContext.Current, owner.__DotCcRuntime) ? (void*)71 : null;

    // Probe the CLR's own stack guard before each modest allocation. This does
    // not provoke a StackOverflowException or depend on a fixed frame count.
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static int ProbeStack(int depth)
    {
        if (depth >= 4096 || !RuntimeHelpers.TryEnsureSufficientExecutionStack()) return depth;
        byte* keep = stackalloc byte[4096];
        keep[0] = 1; keep[4095] = 2;
        int result = ProbeStack(depth + 1);
        return result + keep[0] + keep[4095] - 3;
    }

    private static void* StackWorker(void* argument) => (void*)ProbeStack(0);
    private static void* OwnedStackWorker(Owner owner, void* argument) => StackWorker(argument);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Configured_stack_size_changes_real_worker_stack_capacity(bool explicitOwner)
    {
        using var owner = new Owner();
        using var binding = owner.__DotCcRuntime.Enter();
        int attribute = 0;
        pthread_attr_init(&attribute).ShouldBe(0);
        int[] depths = new int[2];
        try
        {
            for (int index = 0; index < 2; ++index)
            {
                pthread_attr_setstacksize(&attribute, (ulong)(index == 0 ? 1024 * 1024 : 4 * 1024 * 1024)).ShouldBe(0);
                long thread = 0;
                (explicitOwner ? pthread_create(owner, &thread, &attribute, &OwnedStackWorker, null)
                    : pthread_create(&thread, &attribute, &StackWorker, null)).ShouldBe(0);
                void* result = null;
                pthread_join(thread, &result).ShouldBe(0);
                depths[index] = (int)(nint)result;
            }
            depths[0].ShouldBeGreaterThan(0);
            depths[1].ShouldBeGreaterThan(depths[0] * 2);
        }
        finally { pthread_attr_destroy(&attribute).ShouldBe(0); }
    }
}
