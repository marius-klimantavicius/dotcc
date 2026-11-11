using System.Runtime.CompilerServices;
using Cui.Managed;
using Cui.ManagedGallery;
using static Managed.Forms.Cui;

internal static unsafe partial class Program
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_codeSource")]
    private static extern ref TextBuffer CodeSource(GalleryDemo demo);
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_codeEditorResult")]
    private static extern ref CodeEditorResult CodeResult(GalleryDemo demo);

    private static void CodeEditorInteractions(Session s, OsInput input)
    {
        Select(s, input, 16);
        string original = CodeSource(s.State.Demo).Text;
        uint editorId;
        fixed (byte* scope = "Code"u8) cui_push_id(s.App.Ui, scope);
        try { editorId = s.Id("editable-code"); }
        finally { cui_pop_id(s.App.Ui); }
        Click(s, input, s.Node(editorId)->rect, .4f, .15f);
        s.Until(() => CodeResult(s.State.Demo).Focused, "Code editor accepts keyboard focus");
        Key(s, "ctrl+a");
        input.Text("int value = 42;");
        s.Until(() => input.Idle && CodeSource(s.State.Demo).Text == "int value = 42;", "Code editor replaces selected code");
        Key(s, "End"); Key(s, "Return"); Key(s, "Tab");
        input.Text("return value;");
        const string edited = "int value = 42;\n    return value;";
        s.Until(() => input.Idle && CodeSource(s.State.Demo).Text == edited, "Code editor inserts a newline and indentation");
        Check(CodeResult(s.State.Demo).Line == 2 && CodeResult(s.State.Demo).Column == 18,
            "Code editor status follows the caret");
        Key(s, "BackSpace");
        s.Until(() => CodeSource(s.State.Demo).Text == edited[..^1], "Code editor deletes text");
        Key(s, "ctrl+z");
        s.Until(() => CodeSource(s.State.Demo).Text == edited, "Code editor undoes deletion");
        Key(s, "ctrl+f"); input.Text("value");
        s.Until(() => input.Idle && CodeResult(s.State.Demo).FindMatches == 2, "Code editor finds both matches");
        Key(s, "Escape");
        Key(s, "ctrl+g"); Key(s, "ctrl+a"); input.Text("1");
        s.Until(() => input.Idle, "Code editor receives go-to-line input");
        Key(s, "Return");
        s.Until(() => CodeResult(s.State.Demo).Line == 1, "Code editor goes to line one");
        Select(s, input, 0); Select(s, input, 16);
        Check(CodeSource(s.State.Demo).Text == edited, "Edited code survives page navigation");
        Click(s, input, s.Text("Reset code"));
        s.Until(() => CodeSource(s.State.Demo).Text == original, "Reset restores the sample code");
    }

    private static int CodeEditorOnly()
    {
        using (var s = new Session())
        using (var input = OsInput.Create())
        {
            s.Settle(); input.Focus();
            CodeEditorInteractions(s, input);
        }
        Console.WriteLine($"PASS code-editor OS input and cleanup; {assertions} assertions");
        return 0;
    }
}
