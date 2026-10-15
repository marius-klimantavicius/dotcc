/* Independent native physical-layout observer only. These declarations are
   compiled by the native ABI oracle. No translated application body lives here. */
struct NativeBits {
    unsigned prefix : 3;
    signed signed_part : 9;
    unsigned payload : 13;
    unsigned : 0;
    unsigned tail : 5;
};
struct NativeSmall { short left; unsigned char right; };
struct __attribute__((packed)) NativePacked { unsigned char lead; unsigned value; };
struct NativeNode { int value; struct NativeNode *next; int *element; };

int native_bits_signed(const void *storage) {
    return ((const struct NativeBits *)storage)->signed_part;
}
unsigned native_bits_payload(const void *storage) {
    return ((const struct NativeBits *)storage)->payload;
}
unsigned native_bits_tail(const void *storage) {
    return ((const struct NativeBits *)storage)->tail;
}
unsigned native_packed_value(const void *storage) {
    return ((const struct NativePacked *)storage)->value;
}
int native_row_left(const void *storage, int index) {
    return ((const struct NativeSmall *)storage)[index].left;
}
int native_row_right(const void *storage, int index) {
    return ((const struct NativeSmall *)storage)[index].right;
}
int native_node_cycle(const void *storage) {
    const struct NativeNode *node = storage;
    return node->next->next == node;
}
int native_node_element(const void *storage) {
    const struct NativeNode *node = storage;
    return *node->element;
}
