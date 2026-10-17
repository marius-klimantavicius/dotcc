using Application = Translated.Program;

long[] expected = [14, 23, 15, 0, 13, 15, 1, 0, -7, 7, 1, 0, 0, 1,
    0, 1, 30, 20, 96, 23, 17, 0, 1, 19, 7, 5, 6, 123456, 96, 10];
var result = Application.fixture();
if (result != expected.Length || Application.expression_observation_count() != expected.Length)
    throw new InvalidOperationException("Expression result/count changed.");
for (var index = 0; index < expected.Length; ++index)
    if (Application.expression_observe(index) != expected[index])
        throw new InvalidOperationException($"Expression observation {index} changed.");
if (Application.expression_event_count() != 6) throw new InvalidOperationException("Expression event count changed.");
for (var index = 0; index < 6; ++index)
    if (Application.expression_event(index) != index + 1)
        throw new InvalidOperationException("Expression effect ordering changed.");
// This is a target policy check; the native oracle never executes an invalid C
// shift. The count conversion must remain checked even in unchecked arithmetic.
try
{
    _ = Application.target_shift(1UL, 0x80000000U, 0U);
    throw new InvalidOperationException("An oversized target count escaped checked int conversion.");
}
catch (OverflowException) { }
Console.WriteLine($"result={result}\nobservations={expected.Length}");
for (var index = 0; index < expected.Length; ++index) Console.WriteLine($"{index}:{expected[index]}");
Console.WriteLine("events=6");
for (var index = 0; index < 6; ++index) Console.WriteLine($"event{index}:{index + 1}");
