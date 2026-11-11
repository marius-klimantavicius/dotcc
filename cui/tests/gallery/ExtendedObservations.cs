using OriginalGallery = Managed.Forms.Gallery.Gallery;

// Static storage suffix: full SHA256 of the stable logical path examples/gallery_more.c.
internal static unsafe partial class Program
{
    private static string BioText(Session s) { fixed (byte* p = s.State.bio) return Utf8(p); }
    private static int GridSelectedCount(Session s)
    {
        int count = 0;
        var selection = OriginalGallery.Globals.people_sel__unit_5126CC18F9FD396FC1844DADF06211CBC693B528859E5C9A5E0DBF829401BDF2;
        for (int row = 0; row < 2000; row++) if (selection[row] != 0) count++;
        return count;
    }
    private static bool GridSelectionAt(Session s, int row) => OriginalGallery.Globals.people_sel__unit_5126CC18F9FD396FC1844DADF06211CBC693B528859E5C9A5E0DBF829401BDF2[row] != 0;
    private static int GridSortColumn(Session s) => OriginalGallery.Globals.people_sort__unit_5126CC18F9FD396FC1844DADF06211CBC693B528859E5C9A5E0DBF829401BDF2.column;
    private static string GridName(Session s, int row)
    {
        var people = OriginalGallery.Globals.people__unit_5126CC18F9FD396FC1844DADF06211CBC693B528859E5C9A5E0DBF829401BDF2;
        var order = OriginalGallery.Globals.order_ix__unit_5126CC18F9FD396FC1844DADF06211CBC693B528859E5C9A5E0DBF829401BDF2;
        return Utf8(people[order[row]].name);
    }
}
