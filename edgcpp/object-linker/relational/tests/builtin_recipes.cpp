template<class T, class U,
         bool Same = __is_same(T, U),
         bool Trivial = __is_trivial(T),
         bool Constructible = __is_constructible(T, U)>
struct TraitDefaults {};

extern void browse_traits(TraitDefaults<int, int>*);

constexpr bool folded_same = __is_same(int, int);
constexpr bool folded_different = __is_same(int, double);
constexpr bool folded_trivial = __is_trivial(int);
constexpr bool folded_constructible = __is_constructible(int, int);

int main() {
    static_assert(folded_same && !folded_different, "selected builtin results");
    return folded_same && !folded_different && folded_trivial && folded_constructible ? 0 : 1;
}
