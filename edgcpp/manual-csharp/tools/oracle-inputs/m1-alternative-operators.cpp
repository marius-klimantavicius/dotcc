#if true and not false && ((3 bitand 1) == 1) && ((2 bitor 1) == 3) && ((3 xor 1) == 2) && ((compl 0) == -1) && (1 not_eq 2)
constexpr int value = 1;
#else
#error incorrect alternative operator evaluation
#endif
static_assert(value == 1);
