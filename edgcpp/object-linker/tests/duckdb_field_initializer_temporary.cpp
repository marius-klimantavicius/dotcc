extern "C" int putchar(int);
int field_temporary_fail;
struct FieldTemporaryValue {
    int id;
    FieldTemporaryValue(int value) : id(value) {
        putchar('0' + id);
        if (id == field_temporary_fail) throw id;
    }
    ~FieldTemporaryValue() { putchar('a' + id - 1); }
};
struct FieldTemporarySink {
    FieldTemporarySink(const FieldTemporaryValue (&)[2]) { putchar('S'); }
    ~FieldTemporarySink() { putchar('s'); }
};
struct FieldTemporaryHolder {
    FieldTemporarySink sink = {{1, 2}};
};
int main() {
    field_temporary_fail = 2;
    try { FieldTemporaryHolder failed; } catch (int) { putchar('!'); }
    field_temporary_fail = 0;
    { FieldTemporaryHolder complete; }
}
