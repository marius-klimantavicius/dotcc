using System.Globalization;
using System.Text;
using static Managed.Cui.Cui;

internal static unsafe class GalleryObservation
{
    internal static void WriteLayout(string path, cui_ctx* ui)
    {
        var layout = new StringBuilder();
        for (int i = 0; i < ui->node_count; i++)
        {
            var n = &ui->nodes[i];
            layout.Append(FormattableString.Invariant($"{i}\t{n->id}\t{n->parent}\t{n->rect.x:R},{n->rect.y:R},{n->rect.w:R},{n->rect.h:R}\t{n->font}\t{n->font_size:R}\t{n->text_len}\n"));
        }
        File.WriteAllText(path, layout.ToString());
    }
}
