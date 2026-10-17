using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using RoslynObj.CSharp;

if (args.Length != 2) throw new ArgumentException("OUTPUT RUNTIME_PROJECT");
var assembly = typeof(CSharpEmitter).Assembly;
var emissionType = typeof(CSharpEmitter).GetNestedType("Emission", BindingFlags.NonPublic)!;
// These probes exercise only syntax nodes with intrinsic C# types. The renderer
// does not need a captured model or emitter registration state for those nodes.
var emission = RuntimeHelpers.GetUninitializedObject(emissionType);
var render = emissionType.GetMethod("RenderExpression", BindingFlags.Instance | BindingFlags.NonPublic)!;
var integer = Node("CsType", null, 0, "int");
var floating = Node("CsType", null, 0, "double");
var unsigned = Node("CsType", null, 0, "uint");
var boolean = Node("CsType", null, 0, "bool");
var a = Variable(integer, "a"); var b = Variable(integer, "b"); var c = Variable(integer, "c");
var x = Variable(floating, "x"); var y = Variable(floating, "y"); var z = Variable(floating, "z");
var u = Variable(unsigned, "u"); var v = Variable(unsigned, "v");
var flag = Variable(boolean, "flag");
var probes = new List<(string Name, string Text, long Value)>();

Check("multiply-precedence", Binary(integer, "+", a, Binary(integer, "*", b, c)), "unchecked(a + b * c)", 90);
Check("add-grouping", Binary(integer, "*", Binary(integer, "+", a, b), c), "unchecked((a + b) * c)", 210);
Check("subtract-right", Binary(integer, "-", a, Binary(integer, "-", b, c)), "unchecked(a - (b - c))", 23);
Check("subtract-left", Binary(integer, "-", Binary(integer, "-", a, b), c), "unchecked(a - b - c)", 13);
Check("divide-right", Binary(integer, "/", a, Binary(integer, "/", b, c)), "unchecked(a / (b / c))", 15);
Check("remainder-right", Binary(integer, "%", a, Binary(integer, "%", b, c)), "unchecked(a % (b % c))", 0);
Check("float-right", Binary(floating, "+", x, Binary(floating, "+", y, z)), "unchecked(x + (y + z))", 0);
Check("float-left", Binary(floating, "+", Binary(floating, "+", x, y), z), "unchecked(x + y + z)", 1);
Check("unary-minus", Unary(integer, "-", Unary(integer, "-", a)), "- -a", 30);
Check("unary-plus", Unary(integer, "+", Unary(integer, "+", a)), "+ +a", 30);
Check("negative-leaf", Unary(integer, "-", Leaf(integer, "-1", "Unary")), "- -1", 1);
Check("opaque-stable-leaf", Binary(integer, "*", Leaf(integer, "a + b"), c), "unchecked((a + b) * c)", 210);
Check("cast-add", Node("CsCast", integer, Binary(integer, "+", a, b)), "unchecked((int)(a + b))", 42);
Check("cast-unary", Node("CsCast", integer, Unary(integer, "-", a)), "unchecked((int)(-a))", -30);
Check("choice-operand", Binary(integer, "-", Node("CsChoice", integer, flag, a, b), c), "unchecked((flag ? a : b) - c)", 25);
Check("choice-right", Node("CsChoice", integer, flag, a, Node("CsChoice", integer, flag, b, c)), "flag ? a : flag ? b : c", 30);
Check("wrap-add", Binary(unsigned, "+", u, v), "unchecked(u + v)", 1);
Check("narrow-wrap", Node("CsCast", Node("CsType", null, 0, "byte"), Binary(unsigned, "+", u, v)), "unchecked((byte)(u + v))", 1);
Check("checked-count-wrap", Node("CsTargetShift", Node("CsType", null, 0, "ulong"), false, Leaf(unsigned, "1UL", "Primary"), Binary(unsigned, "+", u, v)),
    "TargetArithmetic.ShiftLeft(1UL, checked((int)unchecked(u + v)))", 2);

var program = new StringBuilder("using RoslynObj.CSharp.Runtime;\nint a = 30, b = 12, c = 5;\n"
    + "double x = 1e16, y = -1e16, z = 1.0;\nuint u = uint.MaxValue, v = 2;\nbool flag = true;\n");
foreach (var probe in probes)
    program.Append("if ((long)(").Append(probe.Text).Append(") != ").Append(probe.Value)
        .Append(") throw new System.InvalidOperationException(\"").Append(probe.Name).AppendLine("\");");
program.Append("System.Console.WriteLine(\"rendererChecks=").Append(probes.Count).AppendLine("\");");
Directory.CreateDirectory(args[0]);
File.WriteAllText(Path.Combine(args[0], "Program.cs"), program.ToString());
File.WriteAllText(Path.Combine(args[0], "Probes.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\">\n"
    + "<PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>"
    + "<ImplicitUsings>enable</ImplicitUsings><CheckForOverflowUnderflow>true</CheckForOverflowUnderflow></PropertyGroup>\n"
    + "<ItemGroup><ProjectReference Include=\"" + SecurityElement.Escape(Path.GetFullPath(args[1])) + "\" /></ItemGroup>\n</Project>\n");
Console.WriteLine($"rendererChecks={probes.Count}");

object Node(string name, params object?[] arguments) => Activator.CreateInstance(assembly.GetType("RoslynObj.CSharp." + name)!,
    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, arguments, null)!;
object Variable(object type, string name) => Node("CsVariable", type, name);
object Binary(object type, string operation, object left, object right) => Node("CsBinary", type, operation, left, right);
object Unary(object type, string operation, object operand) => Node("CsUnary", type, operation, operand);
object Leaf(object type, string text, string precedence = "Opaque") => Node("CsLeaf", type, text, true,
    Enum.Parse(assembly.GetType("RoslynObj.CSharp.CsPrecedence")!, precedence));
void Check(string name, object expression, string expected, long value)
{
    var actual = (string)render.Invoke(emission, [expression])!;
    if (actual != expected) throw new InvalidOperationException($"{name}: expected {expected}; got {actual}");
    probes.Add((name, actual, value));
}
