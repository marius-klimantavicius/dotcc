//type:fp
//options_all:--gnu_version=140200 --c++20
//remark:[6.8] Incorrect recording of type constraint for abbreviated friend templates
// 9/30/25  [EDGcpfe/28406]
//
// Incorrect recording of type constraint for abbreviated friend templates
//
// This previously triggered a memory error because the constraint C<T, I> was
// not correctly substituted in the friend definition introduced by X<int, 42>.
// That is now fixed.  (In other similar examples, different error modes could
// occur, such as spurious "duplicate definition" errors induced by the friend
// template substitution.)
template<typename, typename, int> concept C = true;
template<typename T, int I> struct X {
  friend constexpr auto operator+(C<T, I> auto) {
    return true;
  }
};
static_assert(+X<int, 42>{});
