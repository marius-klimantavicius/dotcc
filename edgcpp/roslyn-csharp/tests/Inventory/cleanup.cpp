extern void may_throw(int value);

struct Tracked {
    int value;
    explicit Tracked(int input) : value(input) { may_throw(input); }
    ~Tracked() { may_throw(-value); }
};

int cleanup_context(int input)
{
    Tracked outer(input);
    try {
        Tracked inner(input + 1);
        may_throw(input + 2);
    } catch (int value) {
        return outer.value + value;
    }
    return outer.value;
}
