using Application = Translated.Program;

if (args.Length != 1) throw new ArgumentException("Expected branches | places | callee | aggregate-snapshot.");
var (result, events, values) = args[0] switch
{
    "branches" => (41, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 20, 30, 21, 31, 22, 32, 23, 40 }, new[] { 21 }),
    "places" => (15, new[] { 2, 1, 5, 6, 6, 6, 7, 6, 6, 6, 9, 8, 11, 10 }, new[] { 23, -14, -13, -13, 15, 0, 0, 6, 22, -12, 1 }),
    "callee" => (18, new[] { 1, 2, 3, 1, 2, 3 }, new[] { 6 }),
    "aggregate-snapshot" => (248, new[] { 1, 2, 1, 2 }, new[] { 7, 11, 13, 17, 99, 101 }),
    "aggregate" => throw new InvalidOperationException("Original aggregate candidate has an unresolved retained-EDG/native-GCC disagreement."),
    _ => throw new ArgumentException("Unknown fixture.")
};
for (var iteration = 0; iteration < 2; ++iteration)
{
    var actualResult = Application.fixture();
    var count = Application.sequence_count();
    if (actualResult != result || count != events.Length) throw new InvalidOperationException("Sequencing result/count changed.");
    for (var index = 0; index < events.Length; ++index)
        if (Application.sequence_event(index) != events[index]) throw new InvalidOperationException($"Event {index} changed.");
    for (var index = 0; index < values.Length; ++index)
        if (Application.sequence_value(index) != values[index]) throw new InvalidOperationException($"Value {index} changed.");
}
Console.WriteLine($"result={result}");
Console.WriteLine($"events={events.Length}");
for (var index = 0; index < events.Length; ++index) Console.WriteLine($"event:{index}:{events[index]}");
for (var index = 0; index < values.Length; ++index) Console.WriteLine($"value:{index}:{values[index]}");
