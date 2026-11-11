using System.Text;
using static Managed.Cui.Cui;

internal static unsafe partial class Program
{
    private static uint ExtendedId(Session s, string scope, string id)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(scope + '\0');
        fixed (byte* p = bytes) cui_push_id(s.App.Ui, p);
        try { return s.Id(id); } finally { cui_pop_id(s.App.Ui); }
    }
    private static bool PopupOpen(Session s, string id)
    {
        uint value = s.Id(id);
        for (int i = 0; i < s.App.Ui->popup_count; i++)
            if (s.App.Ui->popups[i].id == value) return s.App.Ui->popups[i].open != 0;
        return false;
    }
    private static cui_rect GridRowRect(Session s, int row)
    {
        uint id = ExtendedId(s, "Data grid", "people");
        return s.Node(cui__hash(&row, sizeof(int), id ^ 0x20Bu))->rect;
    }
    private static void ExtendedInteractions(Session s, OsInput input)
    {
        Select(s, input, 2); Reveal(s, input, "About");
        Click(s, input, s.Rect("bio"), .5f, .65f);
        input.Key(InputKey.Control, true); Key(s, "a"); input.Key(InputKey.Control, false); s.Settle();
        input.Text("Managed multiline editor");
        s.Until(() => input.Idle && BioText(s) == "Managed multiline editor", "OS replaces textarea content");
        Key(s, "Return"); input.Text("second line");
        s.Until(() => input.Idle && BioText(s) == "Managed multiline editor\nsecond line", "OS edits a second textarea line");
        Select(s, input, 1); Select(s, input, 2);
        Check(BioText(s) == "Managed multiline editor\nsecond line", "Multiline editor persists across navigation");

        Select(s, input, 5); Reveal(s, input, "Filters"); Click(s, input, s.Text("Filters"));
        s.Until(() => PopupOpen(s, "filters"), "OS opens filter popover");
        Click(s, input, s.Text("Only active"));
        s.Until(() => s.State.newsletter != 0, "Popover checkbox updates application state");
        Key(s, "Escape"); s.Until(() => !PopupOpen(s, "filters"), "Escape closes filter popover");

        Select(s, input, 15);
        Check(GridSelectedCount(s) == 0, "Grid starts with no selected row");
        Click(s, input, GridRowRect(s, 1), .7f);
        s.Until(() => GridSelectedCount(s) == 1 && GridSelectionAt(s, 1), "OS grid row selects position one");
        input.Key(InputKey.Control, true); Key(s, "a"); input.Key(InputKey.Control, false); s.Settle();
        Check(GridSelectedCount(s) == 2000, "OS Ctrl+A selects all 2,000 virtualized rows");
        Key(s, "Escape"); Check(GridSelectedCount(s) == 0, "OS Escape clears grid selection");
        Click(s, input, GridRowRect(s, 1), .7f);
        Key(s, "Return"); input.Text("Managed grid edit");
        s.Until(() => input.Idle && GridName(s, 1) == "Managed grid edit", "OS enters grid cell editor");
        Key(s, "Return"); Check(GridName(s, 1) == "Managed grid edit", "Enter commits grid edit");
        Key(s, "Return"); input.Text("cancelled edit");
        s.Until(() => input.Idle && GridName(s, 1) == "cancelled edit", "OS changes grid edit draft");
        Key(s, "Escape"); Check(GridName(s, 1) == "Managed grid edit", "Escape restores grid edit backup");
        string before = GridName(s, 1);
        Click(s, input, s.Text("Name"));
        s.Until(() => GridSortColumn(s) == 0 && GridName(s, 1) != before, "OS sorts grid by Name");
        Check(GridSelectedCount(s) == 1 && GridSelectionAt(s, 1), "Grid selection follows row position through sorting, preserving upstream behavior");
        Console.WriteLine($"EXTENDED_STATE\tgridSelected={GridSelectedCount(s)}\tselectedPosition=1\tsortColumn={GridSortColumn(s)}\trowName={GridName(s, 1)}\tbio={Convert.ToHexString(Encoding.UTF8.GetBytes(BioText(s)))}");
        actions.Add("OS multiline editor, filter popover, 2,000-row selection, grid edit commit/cancel and row-position sort parity");
    }
}
