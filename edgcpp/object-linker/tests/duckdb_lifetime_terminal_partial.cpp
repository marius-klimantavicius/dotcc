extern "C" int putchar(int);
static int fail;
struct Value {
    int id;
    Value(int value) : id(value) { putchar('0' + id); if (id == fail) throw id; }
    ~Value() { putchar('a' + id - 1); }
};
static void consume(const Value (&values)[3]) { if (values[2].id == 3) putchar('X'); }
int main() {
    for (fail = 0; fail != 4; ++fail) {
        try { consume({1, 2, 3}); }
        catch (int) { putchar('!'); }
    }
}
