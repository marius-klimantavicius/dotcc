struct Entry { int tag; const char *text; };
struct Node { struct Node *next; const char *text; };

const char *live_strings[] = { "alpha", "beta", "gamma" };
const struct Entry *compound_entry = &(const struct Entry){ 7, "compound" };
extern struct Node node_b;
struct Node node_a = { &node_b, "cycle-a" };
struct Node node_b = { &node_a, "cycle-b" };
struct Node self_node = { &self_node, "self" };

int address_closure(int index)
{
    return live_strings[index][0] + compound_entry->tag
        + node_a.next->text[0] + node_b.next->text[0] + self_node.next->text[0];
}
