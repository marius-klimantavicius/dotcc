using System.Globalization;

namespace DotObj;

internal sealed partial class Emitter
{
    // C permits jumps into a lexical region; C# does not. Only functions with
    // such jumps use this representation. Hoisted storage keeps its source
    // identifier, while initialization stays exactly at its original position.
    private void FlatControl(ObjectFile obj, FunctionBody body, int indent, Names allocator,
        Dictionary<long, string> bindings, Func<Node, string> expression, Func<Node, string> truth,
        Action<int, string> line, Action<Node, int> statement, Action<Node, int> discard)
    {
        var exits = new Dictionary<long, string>();
        var continuations = new Dictionary<long, string>();
        var cases = new Dictionary<long, string>();
        string Label(string name) => allocator.Allocate("__dot_" + name, "flow");
        foreach (Binding binding in body.Bindings.Values.Where(value => value.Kind == "local").OrderBy(value => value.Id))
            line(indent, Type(obj.Type(binding.TypeId)) + " " + bindings[binding.Id] + " = default;");
        line(indent, "// Source jumps enter lexical regions; initialization remains at each source declaration.");
        void Emit(Node node)
        {
            Node Child(string role) => body.Child(node.Id, role);
            bool Has(string role) => body.Children(node.Id, role).Any();
            void Mark(string label) => line(indent, label + ":;");
            void Go(string label) => line(indent, "goto " + label + ";");
            switch (node.Kind)
            {
                case "block":
                    foreach (Edge edge in body.Children(node.Id, "statement")) Emit(body.Nodes[edge.ChildId]);
                    break;
                case "declare":
                    if (Has("initializer")) line(indent, bindings[node.BindingId!.Value] + " = " + expression(Child("initializer")) + ";");
                    break;
                case "if":
                    string next = Label("else"), done = Has("else") ? Label("end_if") : next;
                    line(indent, "if (!(" + truth(Child("condition")) + ")) goto " + next + ";");
                    Emit(Child("then"));
                    if (Has("else")) { Go(done); Mark(next); Emit(Child("else")); }
                    Mark(done);
                    break;
                case "while":
                case "do-while":
                case "for":
                    string start = Label("loop"), condition = Label("condition"), end = Label("end_loop");
                    string step = node.Kind == "for" ? Label("step") : condition;
                    exits.Add(node.Id, end); continuations.Add(node.Id, step);
                    if (node.Kind == "for" && Has("initializer")) discard(Child("initializer"), indent);
                    if (node.Kind != "do-while")
                    {
                        Mark(condition);
                        if (Has("condition")) line(indent, "if (!(" + truth(Child("condition")) + ")) goto " + end + ";");
                    }
                    Mark(start); Emit(Child("body"));
                    if (node.Kind == "for")
                    {
                        Mark(step);
                        if (Has("step")) discard(Child("step"), indent);
                    }
                    if (node.Kind == "do-while")
                    {
                        Mark(condition); line(indent, "if (" + truth(Child("condition")) + ") goto " + start + ";");
                    }
                    else Go(condition);
                    Mark(end);
                    break;
                case "switch":
                    string switchEnd = Label("end_switch"); exits.Add(node.Id, switchEnd);
                    Node selector = Child("condition");
                    Node[] labels = body.Children(Child("body").Id, "statement").Select(edge => body.Nodes[edge.ChildId]).Where(value => value.Kind == "case").ToArray();
                    foreach (Node item in labels) cases.Add(item.Id, Label("case"));
                    line(indent, "switch (" + expression(selector) + ")"); line(indent, "{");
                    foreach (Node item in labels)
                    {
                        string tag = "default";
                        if (body.Children(item.Id, "value").Any())
                        {
                            Node value = body.Child(item.Id, "value");
                            string valueType = obj.Type(selector.TypeId!.Value);
                            string literal = ScalarTypes.SwitchConstant(obj.Type(value.TypeId!.Value), value.ConstantText!, valueType);
                            tag = "case " + ScalarTypes.Literal(valueType, literal);
                        }
                        line(indent + 1, tag + ": goto " + cases[item.Id] + ";");
                    }
                    if (!labels.Any(item => !body.Children(item.Id, "value").Any())) line(indent + 1, "default: goto " + switchEnd + ";");
                    line(indent, "}"); Emit(Child("body")); Mark(switchEnd);
                    break;
                case "case": Mark(cases[node.Id]); break;
                case "break": Go(exits[Child("target").Id]); break;
                case "continue": Go(continuations[Child("target").Id]); break;
                default: statement(node, indent); break;
            }
        }
        Emit(body.Nodes[body.RootId]);
    }
}
