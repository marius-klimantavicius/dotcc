extern "C" int putchar(int);
struct EmptyDestruction {
    EmptyDestruction() { putchar('C'); }
    ~EmptyDestruction() {}
};
static EmptyDestruction value;
// Source-only constexpr template storage must retain its declaration/type even
// when stock EDG suppresses its executable definition (libc++ tuple pattern).
template<class T, class U> struct Same { static constexpr bool value = false; };
template<class T> struct Same<T, T> { static constexpr bool value = true; };
template<unsigned N> constexpr unsigned find_match(const bool (&matches)[N]) {
    for (unsigned i = 0; i < N; ++i) if (matches[i]) return i;
    return N;
}
template<class T, class... Args> struct Match {
    static constexpr bool matches[sizeof...(Args)] = { Same<T, Args>::value... };
    static constexpr unsigned index = find_match(matches);
};
static_assert(Match<int, char, int, double>::index == 1, "constexpr semantic result");
int main() { putchar('M'); return 0; }
