using Application = Translated.Program;
using RoslynObj.CSharp.Runtime;

long[] expected = [211, 11121, 305441741, 0x1234567812345678L, 0x3412, 0x78563412, 0x0807060504030201L,
    63, 0, 23, 1, long.MinValue, 0, 42, 1, long.MaxValue, 0, 13, 1, -2, 0, -42,
    1, long.MinValue, 1, long.MinValue, 17, 51, 0x1111, 0x3333, 0x11111111, 0x33333333,
    0x1111111111111111L, 0x3333333333333333L];
var result = Application.fixture();
if (result != expected.Length || Application.intrinsic_count() != expected.Length || Application.intrinsic_trace() != 1)
    throw new InvalidOperationException("Intrinsic result/count/typed-comma activation changed.");
for (var index = 0; index < expected.Length; ++index)
    if (Application.intrinsic_observation(index) != expected[index])
        throw new InvalidOperationException($"Intrinsic exact observation {index} changed.");
Exception? failure = null;
long sum = 0;
var worker = new Thread(() =>
{
    try
    {
        for (var value = 1; value <= 1000; ++value)
        {
            var actual = Application.intrinsic_consume();
            if (actual != value) throw new InvalidOperationException("Published flag/payload handoff changed.");
            sum += actual;
        }
    }
    catch (Exception exception) { failure = exception; }
}) { IsBackground = true };
worker.Start();
for (var value = 1; value <= 1000; ++value) Application.intrinsic_publish(value);
if (!worker.Join(TimeSpan.FromSeconds(30)) || failure is not null || sum != 500500)
    throw new InvalidOperationException("Generated cross-thread flag/payload protocol failed.", failure);
Console.WriteLine($"result={result}");
Console.WriteLine($"observations={expected.Length}");
for (var index = 0; index < expected.Length; ++index) Console.WriteLine($"{index}:{expected[index]}");
Console.WriteLine("trace=1\nhandoffs=1000\nhandoff-sum=500500");

// Explicit invalid-input policies exercise runtime guards without executing UB
// in the independent C oracle or mutating any object artifact.
unsafe
{
    var storage = stackalloc byte[24];
    var aligned = (byte*)(((nuint)storage + 7) & ~(nuint)7);
    var guardChecks = 0;
    try { TargetIntrinsics.AtomicLoad4(aligned + 1, 0); }
    catch (InvalidOperationException) { ++guardChecks; }
    try { TargetIntrinsics.AtomicLoad4(aligned, 2); }
    catch (InvalidOperationException) { ++guardChecks; }
    try { TargetIntrinsics.CountLeadingZeros64(0); }
    catch (InvalidOperationException) { ++guardChecks; }
    if (guardChecks != 3) throw new InvalidOperationException("Intrinsic invalid-input guards changed.");
}
