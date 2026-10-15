using Application = Translated.Program;

if (args.Length != 1) throw new ArgumentException("Expected c | cpp.");
var (expectedResult, expected) = args[0] switch
{
    "c" => (1150, new[] { 3, 8, 7, 12, 18, 9, 1, 1, 1, 1 }),
    "cpp" => (1212, new[] { 4, 16, 7, 10, 6, 1, 1, 16 }),
    _ => throw new ArgumentException("Unknown program fixture.")
};
for (var iteration = 0; iteration < 2; ++iteration)
{
    if (Application.fixture() != expectedResult || Application.program_observation_count() != expected.Length)
        throw new InvalidOperationException("Selected program result or observation count changed.");
    for (var index = 0; index < expected.Length; ++index)
        if (Application.program_observe(index) != expected[index])
            throw new InvalidOperationException($"Selected program observation {index} changed.");
}
Console.WriteLine($"result={expectedResult}");
Console.WriteLine($"observations={expected.Length}");
for (var index = 0; index < expected.Length; ++index) Console.WriteLine($"{index}:{expected[index]}");
