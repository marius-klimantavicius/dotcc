extern "C" int putchar(int);
struct FieldNoEffectValue {
    FieldNoEffectValue() { putchar('C'); }
    ~FieldNoEffectValue() {}
};
struct FieldNoEffectHolder {
    FieldNoEffectValue value = {};
};
int main() {
    FieldNoEffectHolder holder;
    return 0;
}
