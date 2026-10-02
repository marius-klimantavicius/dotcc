//type:fp
//options_all:--c++17
//remark:[5.1] Assertion failure in lower_routine
// 5/24/19  [EDGcpfe/21110]
//
// Assertion failure in lower_routine
//
// In some configurations that use lowering, an assertion failure (in
// lower_routine) had occurred and is now fixed.
auto monoid = [](auto v) { return [=] { return v; }; };
auto add = [](auto m1) constexpr {
  auto ret = m1();
  return [=](auto m2) mutable {
    auto m1val = m1();
    auto plus = [=] (auto m2val) mutable constexpr { return m1val += m2val;};
    ret = plus(m2());
    return monoid(ret);
  };
};
constexpr auto zero = monoid(0);
constexpr auto one = monoid(1);
static_assert(add(one)(zero)() == one());
