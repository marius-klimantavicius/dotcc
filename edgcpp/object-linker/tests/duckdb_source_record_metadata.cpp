// Unused original templates must retain declarations without physical layouts.
template<class Rollback> struct DuckdbSourceOnlyGuard {
    Rollback rollback;
    bool complete = false;
};
template<class Base> struct DuckdbSourceOnlyDerived : Base {
    int marker = 7;
};
struct DuckdbSourceConcrete { int value; };
int main() {
    DuckdbSourceConcrete value{17};
    return value.value == 17 ? 0 : 1;
}
