template<decltype(nullptr) Value> constexpr int accept_null() { return 0; }
template<class T> concept NullValue = requires(T value) { nullptr; accept_null<nullptr>(); value + 7; };
template<NullValue T> struct Holder { T value = {}; };
static_assert(NullValue<int>);
int main() { return 0; }
