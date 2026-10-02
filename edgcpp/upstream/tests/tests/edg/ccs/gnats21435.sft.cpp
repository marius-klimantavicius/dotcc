//type:cp
//options_all:--c++17

struct AGGR {
  short x;
};

void func(AGGR);

template <typename, typename To, typename From>
struct Test {
  static constexpr bool value = false;
};

template <typename To, typename From>
struct Test<decltype( func({From()}) ), To, From> {
  static constexpr bool value = true;
};

struct Empty {};

static_assert(Test<void, AGGR, short>::value);
static_assert(!Test<void, AGGR, float>::value);
static_assert(!Test<void, AGGR, Empty>::value);
