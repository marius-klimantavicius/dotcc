template<class T, bool Quiet = noexcept(T{true})>
struct BracedInitializer {};

template<class T, bool Quiet = noexcept(T())>
struct EmptyInitializer {};

constexpr bool folded_braced = noexcept(int{1});
constexpr bool folded_empty = noexcept(int());

int main() {
    return folded_braced && folded_empty ? 0 : 1;
}
