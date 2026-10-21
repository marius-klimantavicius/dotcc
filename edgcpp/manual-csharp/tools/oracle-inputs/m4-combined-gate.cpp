// Authored native/managed difficult-path contract; no fixture-special semantics.
constexpr long mark(long value) { return value + 3; }
template<class T> constexpr T identity(T value) { T local = value; return local; }
template<class T> constexpr long combine(T value) {
    T copy = identity(value);
    return mark(0) + copy;
}
constexpr int mark(int value) { return value + 100; }
constexpr long run() { int mark = 0; return combine(4); }
static_assert(run() == 7);
static_assert(combine(5L) == 8);
static_assert(combine(4) == 7);
