using System.Globalization;
using System.Numerics;

namespace DotObj;

internal sealed partial class ObjectFile
{
    // The graph remains in SQLite. Integrity is checked with streaming queries;
    // neither loading nor source-map emission materializes or normalizes it.
    private void ReadEdgGraphIntegrity()
    {
        using (var configuration = database.Prepare("SELECT detail FROM edg_graph_coverage WHERE entry_kind=0 AND field='$configuration' AND disposition='configuration'"))
            if (!configuration.Read() || configuration.Text(0).Length == 0 || configuration.Read())
                throw new InvalidDataException("Missing EDG graph configuration and field coverage identity");
        using(var kinds=database.Prepare("SELECT count(*),min(entry_kind),max(entry_kind),count(DISTINCT detail) FROM edg_graph_coverage WHERE field='$entryKind' AND disposition='configuration'"))
            if(!kinds.Read() || kinds.Integer(0)<2 || kinds.Integer(1)!=0 || kinds.Integer(2)!=kinds.Integer(0)-1 || kinds.Integer(3)!=kinds.Integer(0))
                throw new InvalidDataException("Missing or invalid configured EDG entry-kind catalog");
        RejectRows("SELECT n.id FROM edg_graph_nodes n LEFT JOIN edg_graph_coverage k ON k.entry_kind=n.entry_kind AND k.field='$entryKind' AND k.disposition='configuration' WHERE n.entry_kind=0 OR k.entry_kind IS NULL OR k.detail<>n.entry_kind_name", "EDG graph node has an unknown configured entry kind/name");
        RejectRows("SELECT f.node_id FROM edg_graph_fields f LEFT JOIN edg_graph_coverage k ON k.entry_kind=f.target_entry_kind AND k.field='$entryKind' AND k.disposition='configuration' WHERE f.value_kind='reference' AND k.entry_kind IS NULL", "EDG graph reference has an unknown configured target kind");
        RejectRows("SELECT entry_kind FROM edg_graph_nodes GROUP BY entry_kind HAVING count(DISTINCT entry_kind_name)<>1", "EDG graph entry kind has inconsistent names");
        RejectRows("SELECT entry_kind_name FROM edg_graph_nodes GROUP BY entry_kind_name HAVING count(DISTINCT entry_kind)<>1", "EDG graph entry name has inconsistent kinds");
        RejectRows("SELECT node_id FROM edg_graph_fields GROUP BY node_id,name HAVING min(ordinal)<>0 OR max(ordinal)<>count(*)-1", "EDG graph field ordinals are not contiguous");
        // Composite foreign keys checked by the container reader validate every
        // non-null target and its exact EDG entry kind. Cycles/shared edges are
        // valid graph structure and require no traversal or ownership proof.
        using var fields = database.Prepare("SELECT value_kind,value FROM edg_graph_fields WHERE value_kind IN ('signed','unsigned')");
        while (fields.Read())
        {
            string kind = fields.Text(0), encoded = fields.Text(1);
            if (!BigInteger.TryParse(encoded, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value) ||
                value.ToString(CultureInfo.InvariantCulture) != encoded || kind == "unsigned" && value.Sign < 0)
                throw new InvalidDataException("EDG graph primitive has an invalid integer encoding");
        }
    }
}
