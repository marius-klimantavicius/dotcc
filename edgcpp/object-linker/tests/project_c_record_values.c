struct Pair { int first; double second; int *pointer; int (*callback)(int); };
union Word { unsigned long bits; double value; };
struct Pair shared_pair;
static int calls;
static int add(int value) { ++calls; return value + 7; }

struct Pair make_pair(int *pointer) {
    struct Pair result = { 3, 4.5, pointer, add };
    return result;
}
int consume_pair(struct Pair value) {
    value.first += 10;
    return value.first + *value.pointer + value.callback(2) + (int)value.second;
}
struct Pair exchange_pair(struct Pair *destination, struct Pair value) {
    return *destination = value;
}
union Word make_word(unsigned long value) {
    union Word result;
    result.bits = value;
    return result;
}
unsigned long consume_word(union Word value) { return value.bits; }

int main(void) {
    int integer = 11;
    struct Pair first = make_pair(&integer);
    struct Pair second = first;
    if (second.first != 3 || second.second != 4.5 || second.pointer != &integer || second.callback != add) return 1;
    second.first = 20;
    if (first.first != 3) return 2;
    if (consume_pair(first) != 37 || calls != 1 || first.first != 3) return 3;
    struct Pair snapshot = exchange_pair(&shared_pair, second);
    second.first = 99;
    if (snapshot.first != 20 || shared_pair.first != 20) return 4;
    first = second = shared_pair;
    if (first.first != 20 || second.first != 20) return 5;
    union Word word = make_word(0x0123456789abcdefUL);
    union Word copied = word;
    copied.bits = 7;
    if (consume_word(word) != 0x0123456789abcdefUL || copied.bits != 7) return 6;
    return 0;
}
