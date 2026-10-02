//options_all:--c++17
template <class T>
T &&declval() noexcept;
 
template <class Void, class To, class From>
struct IsCopyListInitializableFrom {
              static constexpr bool value = false;
};
 
template <class To, class From>
struct IsCopyListInitializableFrom<decltype(declval<void (*)(To)>()({ declval<From>() })), To, From> {
              static constexpr bool value = true;
};
 
struct AGGR {
              short x;
};
 
struct Empty {};
 
static_assert(!IsCopyListInitializableFrom<void, AGGR, Empty>::value); // <== intellisense shows red squiggles
static_assert(IsCopyListInitializableFrom<void, AGGR, short>::value);
