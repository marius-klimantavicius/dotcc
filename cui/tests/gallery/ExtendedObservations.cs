using OriginalGallery = Managed.Cui.Gallery.Gallery;

internal static unsafe partial class Program
{
    private static string BioText(Session s) { fixed (byte* p = s.State.bio) return Utf8(p); }
    private static int GridSelectedCount(Session s)
    {
        int count = 0;
        var selection = OriginalGallery.Globals.people_sel__unit_171AFF2640DEF384BF9F549504A865FD4BAE78ECD63FED7DAE8D7F3819458C94;
        for (int row = 0; row < 2000; row++) if (selection[row] != 0) count++;
        return count;
    }
    private static bool GridSelectionAt(Session s, int row) => OriginalGallery.Globals.people_sel__unit_171AFF2640DEF384BF9F549504A865FD4BAE78ECD63FED7DAE8D7F3819458C94[row] != 0;
    private static int GridSortColumn(Session s) => OriginalGallery.Globals.people_sort__unit_171AFF2640DEF384BF9F549504A865FD4BAE78ECD63FED7DAE8D7F3819458C94.column;
    private static string GridName(Session s, int row)
    {
        var people = OriginalGallery.Globals.people__unit_171AFF2640DEF384BF9F549504A865FD4BAE78ECD63FED7DAE8D7F3819458C94;
        var order = OriginalGallery.Globals.order_ix__unit_171AFF2640DEF384BF9F549504A865FD4BAE78ECD63FED7DAE8D7F3819458C94;
        return Utf8(people[order[row]].name);
    }
}
