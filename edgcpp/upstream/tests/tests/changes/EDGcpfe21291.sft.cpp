//type:fp
//options_all:--gnu=50400 --c++11
//remark:[5.1] GNU compatibility: __builtin_expect and __builtin_expect_with_probability
// 5/24/19  [EDGcpfe/21291]
//
// GNU compatibility: __builtin_expect and __builtin_expect_with_probability
// accepted in constexpr expressions in C++11 mode
//
// A change has been made to accept calls to __builtin_expect and
// __builtin_expect_with_probability in constexpr contexts in C++11 mode.
constexpr int f(int i) {
  return __builtin_expect(i <= 100, 1) ? 0 : 0;
}
constexpr int l = f(4);
