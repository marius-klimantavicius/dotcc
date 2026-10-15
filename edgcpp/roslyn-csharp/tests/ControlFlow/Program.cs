using Application = Translated.Program;
var first = Observe();
if (Observe() != first) throw new InvalidOperationException("Repeated control-flow observations differ.");
Console.Write(first);
static string Observe()
{
    var result = Application.fixture();
    var count = Application.control_observation_count();
    if (count < 0 || count > 32) throw new InvalidOperationException("Observation count is outside fixture storage.");
    return $"result={result}\nobservations={count}\n" +
        string.Concat(Enumerable.Range(0, count).Select(index => $"{index}:{Application.control_observe(index)}\n"));
}
