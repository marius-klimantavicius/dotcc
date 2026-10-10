struct Handle;
struct Base { long value; };
struct Derived : Base { long other; };
long consume(const Base& base) { return base.value; }
long read(Handle* handle) { return consume(*reinterpret_cast<Derived*>(handle)); }
int main() { Derived value; value.value=42; value.other=11; return read(reinterpret_cast<Handle*>(&value))==42 ? 0 : 1; }
