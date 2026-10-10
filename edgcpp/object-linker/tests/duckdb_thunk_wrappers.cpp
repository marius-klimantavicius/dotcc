// Lowered wrappers reached through DuckDB stringstream vtables, reduced to
// independent fixed/virtual receiver adjustments and covariant returns.
// Explicit constructors avoid the independent implicit-constructor aggregate
// initialization gate (storage initialization must not become a record copy).
namespace fixed_thunk {
struct Left { virtual int value(int n) { return n + 1; } };
struct Right { virtual int value(int n) { return n + 2; } };
struct Derived : Left, Right { Derived() {} int value(int n) override { return n + 3; } };
int probe() { Derived value; Right *base = &value; return base->value(7); }
}
namespace virtual_thunk {
struct Base { virtual int value(int n) { return n + 1; } int pad; };
struct Derived : virtual Base { int value(int n) override { return n + 4; } };
int probe() { Derived value; Base *base = &value; return base->value(7); }
}
namespace covariant_thunk {
struct Base {
    virtual Base *pointer(bool empty) { return empty ? nullptr : this; }
    virtual Base &reference() { return *this; }
};
struct Padding { int padding; };
struct Derived : Padding, virtual Base {
    Derived *pointer(bool empty) override { return empty ? nullptr : this; }
    Derived &reference() override { return *this; }
};
int probe() {
    Derived value;
    Base *base = &value;
    return base->pointer(false) == base && base->pointer(true) == nullptr && &base->reference() == base;
}
}
namespace destructor_thunk {
int destroyed;
struct Left { virtual ~Left() { destroyed += 1; } };
struct Right { virtual ~Right() { destroyed += 2; } };
struct Derived : Left, Right { Derived() {} ~Derived() override { destroyed += 4; } };
int probe() { destroyed = 0; { Derived value; } return destroyed; }
}
extern "C" int duckdb_thunk_probe() {
    return fixed_thunk::probe() == 10 && virtual_thunk::probe() == 11 &&
        covariant_thunk::probe() == 1 && destructor_thunk::probe() == 7 ? 0 : 1;
}
#ifdef DUCKDB_THUNK_NATIVE_ORACLE
int main() { return duckdb_thunk_probe(); }
#endif
