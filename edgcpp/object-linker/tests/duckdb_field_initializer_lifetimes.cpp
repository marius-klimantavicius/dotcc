extern "C" int putchar(int);
static int fail;
struct Value {
    int id;
    Value(int value) : id(value) {
        putchar('0' + id);
        if (id == fail) throw id;
    }
    ~Value() { putchar('a' + id - 1); }
};
struct Holder {
    Value elements[3] = {1, 2, 3};
    Holder() {}
    explicit Holder(int) {}
    explicit Holder(long) : elements{4, 5, 6} {}
};
// The original initializer graph is still useful when no constructor selects it.
struct UnusedHolder { Value elements[2] = {7, 8}; };
int main() {
    fail = 2;
    try { Holder failed; return 1; }
    catch (int error) { if (error != 2) return 2; putchar('!'); }
    fail = 0;
    { Holder first; }
    { Holder second(1); }
    { Holder explicit_members(1L); }
    return 0;
}
