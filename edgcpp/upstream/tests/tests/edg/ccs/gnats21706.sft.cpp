//type:cp
//options_all:--c++17

struct S {
  S& operator=(const S&);
};

union U {
  S s;
};

static_assert(!__is_trivially_copyable(S));
static_assert(!__is_trivially_copyable(U));
