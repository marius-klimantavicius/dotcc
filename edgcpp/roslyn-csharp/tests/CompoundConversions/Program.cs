using Application = Translated.Program;

long[] expected = [-9223372036854775807L, 1, 15, 11, 2, 4294967294, 16, 2, -2147483392, 4,
    9, 9, 1, 1, 36, 60, 1, 1, 2, 16, 6, 6];
var result = Application.fixture();
if (result != expected.Length || Application.compound_count() != expected.Length)
    throw new InvalidOperationException("Compound conversion result/count changed.");
for (var index = 0; index < expected.Length; ++index)
    if (Application.compound_observe(index) != expected[index])
        throw new InvalidOperationException($"Compound conversion observation {index} changed.");
Console.WriteLine($"result={result}\nobservations={expected.Length}");
for (var index = 0; index < expected.Length; ++index) Console.WriteLine($"{index}:{expected[index]}");
