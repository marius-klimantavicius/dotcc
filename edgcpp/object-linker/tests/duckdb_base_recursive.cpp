// Reduced from libc++ basic_ios/basic_ostream reached by DuckDB's
// StringUtil::GenerateRandomName. Following peer first reaches Derived, whose
// Base subobject must retain direct-base identities before Base is fully emitted.
struct Direct { int direct; };
struct Virtual { int shared; };
struct Derived;
struct Base : Direct, virtual Virtual { Derived *peer; int own; };
struct Derived : Base { int extra; };
extern "C" int duckdb_base_probe(Base *value) {
    return value->direct + value->peer->extra + value->own;
}
#ifdef DUCKDB_BASE_NATIVE_ORACLE
int main() {
    Derived value;
    value.direct = 3;
    value.shared = 13;
    value.peer = &value;
    value.own = 5;
    value.extra = 7;
    return duckdb_base_probe(&value) == 15 ? 0 : 1;
}
#endif
