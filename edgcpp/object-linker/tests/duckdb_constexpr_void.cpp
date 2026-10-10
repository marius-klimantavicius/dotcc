// Reduced from libc++ iterator_operations::__validate_iter_reference/__iter_move.
template<class T> constexpr void validate_reference() { static_assert(sizeof(T) != 0, "complete"); }
template<class T> constexpr T&& iter_move(T* pointer) {
    validate_reference<T>();
    return static_cast<T&&>(*pointer);
}
constexpr void folded_void() {}
static int calls;
static int record(int value) { ++calls; return value; }
static int select(bool condition) {
    condition ? folded_void() : static_cast<void>(record(1));
    return (folded_void(), record(condition ? 3 : 4));
}
int main() {
    int value = 27;
    if (iter_move(&value) != 27) return 1;
    auto&& moved = iter_move(&value);
    moved = 32;
    if (value != 32) return 2;
    if (select(true) != 3 || calls != 1) return 3;
    if (select(false) != 4 || calls != 3) return 4;
    folded_void();
    return 0;
}
