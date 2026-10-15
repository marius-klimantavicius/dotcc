using Application = Translated.Program;

long[] expected = [7, 2147483649, 6, 11, 23, 13, 1, 6, 123456, 55, 8, 1, 24];
var result = Application.fixture();
if (result != expected.Length || Application.last_four_count() != expected.Length)
    throw new InvalidOperationException("Last-four result/count changed.");
for (var index = 0; index < expected.Length; ++index)
    if (Application.last_four_observe(index) != expected[index])
        throw new InvalidOperationException($"Last-four exact observation {index} changed.");
if (Application.last_four_event_count() != 6) throw new InvalidOperationException("A cursor branch/prefix ran more than once.");
for (var index = 0; index < 6; ++index)
    if (Application.last_four_event(index) != index + 1) throw new InvalidOperationException("Cursor prefix order/branch selection changed.");
Console.WriteLine($"result={result}\nobservations={expected.Length}");
for (var index = 0; index < expected.Length; ++index) Console.WriteLine($"{index}:{expected[index]}");
Console.WriteLine("events=6");
for (var index = 0; index < 6; ++index) Console.WriteLine($"event{index}:{index + 1}");
