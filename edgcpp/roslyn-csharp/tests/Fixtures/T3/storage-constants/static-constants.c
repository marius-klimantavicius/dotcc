/* Genuine application input: retained constant recipes and ordinary bodies only. */
struct Pair { int first; int second; };
struct Node { struct Node *next; int value; };
static int observations[15];
int increment_static(int delta) {
    static int value = 7;
    value += delta;
    return value;
}
int sibling_static(void) {
    static int value = 11;
    return ++value;
}
int *static_array(void) {
    static int values[4] = {3, 5};
    ++values[2];
    return values;
}
struct Node *static_node(void) {
    static struct Node node = {&node, 23};
    return &node;
}
const char *literal_bytes(void) { return "A\0Z"; }
struct Pair pair_constant(void) { return (struct Pair){17, 19}; }
int assign_constant(struct Pair *destination) {
    *destination = (struct Pair){31, 37};
    return destination->first + destination->second;
}
int fixture(void) {
    observations[0] = increment_static(2);
    observations[1] = sibling_static();
    int *first = static_array();
    observations[2] = first[0];
    observations[3] = first[1];
    observations[4] = first[2];
    observations[5] = first[3];
    observations[6] = first == static_array();
    observations[7] = first[2];
    struct Node *node = static_node();
    observations[8] = node == node->next;
    observations[9] = node->value;
    const char *text = literal_bytes();
    observations[10] = text[0] + text[1] + text[2] + text[3];
    struct Pair value = pair_constant();
    observations[11] = value.first + value.second;
    observations[12] = assign_constant(&value);
    observations[13] = value.first;
    observations[14] = value.second;
    return observations[0] + observations[1] + observations[4] + observations[7]
        + observations[9] + observations[10] + observations[11] + observations[12];
}
int storage_observation_count(void) { return 15; }
int storage_observe(int index) { return observations[index]; }
