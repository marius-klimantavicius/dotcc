#include <type_traits>
template<class T> constexpr int rank = std::is_pointer_v<T> ? 7 : 3;
static_assert(std::is_same_v<std::remove_const_t<const int>, int>);
static_assert(std::is_trivially_copyable_v<int>);
static_assert(rank<int*> == 7 && rank<long> == 3);
int main() { return std::is_integral_v<long> && std::is_unsigned_v<unsigned> ? 0 : 1; }
