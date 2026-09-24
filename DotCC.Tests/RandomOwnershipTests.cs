using System.Threading;
using Shouldly;
using Xunit;
using RuntimeLibc = DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed class RandomOwnershipTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Seeds_and_sequences_belong_to_the_owner_across_nested_bindings_and_threads(bool posix)
    {
        void Seed(uint seed) { if (posix) RuntimeLibc.srandom(seed); else RuntimeLibc.srand(seed); }
        long Next() => posix ? RuntimeLibc.random() : RuntimeLibc.rand();
        using var first = new RuntimeLibc.RuntimeContext();
        using var second = new RuntimeLibc.RuntimeContext();
        long[] expected = new long[3];
        using (first.Enter())
        {
            Seed(123);
            for (int i = 0; i < expected.Length; i++) expected[i] = Next();
            Seed(123);
            Next().ShouldBe(expected[0]);
            using (second.Enter())
            {
                Seed(123);
                Next().ShouldBe(expected[0]);
                Seed(987);
                Next();
            }
            // A worker bound to this program consumes the same C sequence;
            // another owner's reseed above must not have changed it.
            long workerValue = -1;
            var worker = new Thread(() => { using (first.Enter()) workerValue = Next(); });
            worker.Start();
            worker.Join();
            workerValue.ShouldBe(expected[1]);
            Next().ShouldBe(expected[2]);
        }
    }

    [Fact]
    public void Bound_reseed_does_not_change_legacy_generators()
    {
        RuntimeLibc.srand(77); RuntimeLibc.srandom(77);
        RuntimeLibc.rand(); RuntimeLibc.random();
        int expectedRand = RuntimeLibc.rand();
        long expectedRandom = RuntimeLibc.random();
        RuntimeLibc.srand(77); RuntimeLibc.srandom(77);
        RuntimeLibc.rand(); RuntimeLibc.random();
        using var owner = new RuntimeLibc.RuntimeContext();
        using (owner.Enter())
        {
            RuntimeLibc.srand(88); RuntimeLibc.srandom(88);
            RuntimeLibc.rand(); RuntimeLibc.random();
        }
        RuntimeLibc.rand().ShouldBe(expectedRand);
        RuntimeLibc.random().ShouldBe(expectedRandom);
    }
}
