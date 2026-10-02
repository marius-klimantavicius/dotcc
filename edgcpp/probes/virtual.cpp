struct Base { virtual int value() const { return 1; } };
struct Derived : Base { int value() const override { return 42; } };
int call(const Base& b) { return b.value(); }
int main() { Derived d; return call(d) == 42 ? 0 : 1; }
