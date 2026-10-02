//options_all:--c++20 -A
void q(int *);
int &q(const int *);

template <typename T>
constexpr bool f() {
  T x;
  constexpr bool v = requires {
    [=] { ++q(&x); };
  };
  return v;
}

static_assert(f<int>());  // OK

//cwg: 2911
//title: Unclear meaning of expressions "appearing within" subexpressions
//meeting: Wroclaw 11/24
//edg_status: EDGcpfe/27752
//fixed_in: 6.9
