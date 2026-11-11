using System.Diagnostics;
using static Managed.Cui.Cui;

internal static unsafe partial class Program
{
    private static cui_rect NarrowMenuRect(Session s)
    {
        for (int i = 0; i < s.App.Ui->node_count; i++)
        {
            var node = &s.App.Ui->nodes[i];
            if (node->text != null && node->rect.x < 80 && node->rect.y < 100 && Utf8(node->text) == "")
                return node->rect;
        }
        throw new InvalidOperationException("The narrow gallery header has no menu button");
    }
    private static void ResponsiveInteractions(Session s, OsInput input)
    {
        input.Resize(720, 820);
        s.Until(() => s.App.Ui->vw == 720, "OS resize activates the narrow gallery layout");
        s.Settle(5);
        Check(s.State.drawer == 0, "Narrow page list starts closed");
        Click(s, input, NarrowMenuRect(s));
        s.Until(() => s.State.drawer != 0, "OS menu button opens the page-list sheet");
        s.Settle(100);
        Click(s, input, s.Text("Buttons", sidebar: true));
        s.Until(() => s.State.page == 1 && s.State.drawer == 0 && !s.HasBlocker(), "Drawer navigation selects Buttons and closes the sheet");
        input.Resize(1240, 820);
        s.Until(() => s.App.Ui->vw == 1240, "OS resize restores wide gallery navigation");
        s.Settle(5);
        Check(s.Rect("nav").w > 0 && s.State.drawer == 0 && s.App.Ui->parent_depth == 0,
            "Wide sidebar is restored with balanced layout and closed drawer");
        Console.WriteLine($"NARROW_STATE\tpage={s.State.page}\tdrawer={(int)s.State.drawer}\tviewport={s.App.Ui->vw},{s.App.Ui->vh}");
        actions.Add("OS narrow resize, page drawer open/select/close and wide navigation restore");
    }
    private static int ResponsiveOnly()
    {
        using var s = new Session(); s.Settle();
        using var input = OsInput.Create(); input.Focus();
        ResponsiveInteractions(s, input);
        input.Close(); var timer = Stopwatch.StartNew();
        while (s.Frame()) if (timer.Elapsed > TimeSpan.FromSeconds(8)) throw new TimeoutException("Responsive app close");
        Console.WriteLine($"PASS responsive-only assertions={assertions}");
        return 0;
    }
}
