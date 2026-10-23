using Managed.Security.S2n;
if (args.Length != 2) throw new ArgumentException("Expected native observations and output receipt");
var original = Console.Out;
using var captured = new StringWriter();
int status;
try { Console.SetOut(captured); status = S2nLayout.layout_probe(); }
finally { Console.SetOut(original); }
var actual = captured.ToString().ReplaceLineEndings("\n").Trim();
var expected = File.ReadAllText(args[0]).ReplaceLineEndings("\n").Trim();
using (var output = File.Create(args[1]))
using (var json = new System.Text.Json.Utf8JsonWriter(output))
{
    json.WriteStartObject(); json.WriteBoolean("passed", status == 0 && actual == expected);
    json.WriteNumber("status", status); json.WriteString("actual", actual); json.WriteString("expected", expected); json.WriteEndObject();
}
if (status != 0 || actual != expected) throw new InvalidOperationException("Actual emitted/native s2n layout mismatch: " + args[1]);
Console.WriteLine("Actual emitted/native s2n layouts, offsets and bitfield storage agree.");
