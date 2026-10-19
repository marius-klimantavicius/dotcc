typedef int (*Callback)(int);
typedef int *Address;
struct Token { short value; unsigned short code; int *owner; Callback apply; };
struct Block { unsigned int words[5]; int tail; };
struct Root {
    int seed;
    struct Token *rows;
    struct Token *twin;
    struct Block *blocks;
    Address *addresses;
    Callback *callbacks;
};
extern int later;
int plus(int value) { return value + 3; }
struct Root root = {
    .seed = 7,
    .rows = (struct Token[]) {
        {-17, 65535, &root.seed, plus}, {23, 32768, &later, plus}, {0}
    },
    .twin = (struct Token[]) {
        {-17, 65535, &root.seed, plus}, {23, 32768, &later, plus}, {0}
    },
    .blocks = (struct Block[]) {{{1, 2, 0, 0, 91}, 19}, {{0}, -5}},
    .addresses = (Address[]) {&root.seed, &later, 0},
    .callbacks = (Callback[]) {plus, 0}
};
int later = 19;
_Alignas(64) struct Root aligned_root = {
    .seed = 11, .rows = (struct Token[]) {{5, 32768, &aligned_root.seed, plus}}
};
_Alignas(32) int aligned_scalar = 29;
_Alignas(32) Callback aligned_callback = plus;
_Thread_local struct Token per_thread[] = {{31, 65535, &root.seed, plus}};

int owned_check(void) {
    return root.rows != root.twin && root.rows[0].code == 65535 &&
        root.rows[1].code == 32768 && root.rows[2].value == 0 &&
        root.rows[2].owner == 0 && root.rows[2].apply == 0 &&
        root.rows[0].owner == &root.seed && root.rows[1].owner == &later &&
        root.rows[0].apply == plus && root.rows[1].apply(*root.rows[1].owner) == 22 &&
        root.blocks[0].words[0] == 1 && root.blocks[0].words[1] == 2 &&
        root.blocks[0].words[2] == 0 && root.blocks[0].words[4] == 91 &&
        root.blocks[0].tail == 19 && root.blocks[1].words[4] == 0 && root.blocks[1].tail == -5 &&
        root.addresses[0] == &root.seed && *root.addresses[1] == 19 && root.addresses[2] == 0 &&
        root.callbacks[0] == plus && root.callbacks[0](5) == 8 && root.callbacks[1] == 0 &&
        (unsigned long)&aligned_root % 64 == 0 && aligned_root.seed == 11 &&
        aligned_root.rows[0].owner == &aligned_root.seed && *aligned_root.rows[0].owner == 11 &&
        (unsigned long)&aligned_scalar % 32 == 0 && aligned_scalar == 29 &&
        (unsigned long)&aligned_callback % 32 == 0 && aligned_callback == plus && aligned_callback(5) == 8;
}
int owned_mutate(void) {
    root.rows[0].value = 99;
    return root.twin[0].value == -17 && root.rows[0].value == 99;
}
int owned_first(void) { return root.rows[0].value; }
unsigned long owned_address(void) { return (unsigned long)root.rows; }
int owned_static_step(void) {
    static struct Token local_rows[] = {{41, 65535, &later, plus}};
    static struct Root local = {.rows = local_rows};
    return local.rows[0].apply(local.rows[0].value++);
}
int owned_thread_step(void) {
    return per_thread[0].apply(per_thread[0].value++) + *per_thread[0].owner;
}
unsigned long owned_thread_address(void) { return (unsigned long)per_thread; }
int owned_expressions(int take_first) {
    int counter = 0;
    struct Token *selected = take_first
        ? (struct Token[]) {{++counter, 7, 0, plus}, {++counter, 9, 0, plus}}
        : (struct Token[]) {{10 + ++counter, 11, 0, plus}, {10 + ++counter, 13, 0, plus}};
    return counter == 2 && selected[0].value == (take_first ? 1 : 11) &&
        selected[1].value == (take_first ? 2 : 12) && selected[0].apply(5) == 8;
}
