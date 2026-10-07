/* Header-free differential regressions for the C project emitter. */
typedef struct Bits { unsigned x : 3; signed y : 4; unsigned z : 31; } Bits;

int volatile_operations(int input)
{
    volatile int value = input;
    int pre = ++value;
    int post = value++;
    int assigned = (value = 7);
    int compound = (value += 3);
    return pre * 100000 + post * 10000 + assigned * 1000 + compound * 100 + value;
}

int narrow_division(int input, int divisor)
{
    Bits bits = {0};
    bits.x = input;
    bits.x /= divisor;
    return bits.x;
}

int wide_bitfield_division(int input)
{
    Bits bits = {0};
    bits.z = input;
    bits.z /= -1;
    return bits.z;
}

int signed_bitfield_shift(int input, int shift)
{
    Bits bits = {0};
    bits.y = input;
    bits.y >>= shift;
    return bits.y;
}

static Bits *receiver(Bits *bits, int *calls)
{
    ++*calls;
    return bits;
}

int bitfield_evaluate_once(int input)
{
    Bits bits = {0};
    int calls = 0;
    int result = receiver(&bits, &calls)->x += input;
    int old = receiver(&bits, &calls)->x++;
    return calls * 1000 + result * 100 + old * 10 + bits.x;
}

int twice(int input) { return input * 2; }
typedef struct Callback { int (*call)(int); } Callback;

int callback_comma(int input)
{
    Callback callback = {twice};
    int (*local)(int) = twice;
    int result = 0;
    return (result = callback.call(input), local(result + 1));
}
