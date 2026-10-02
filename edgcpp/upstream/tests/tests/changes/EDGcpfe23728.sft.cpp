//type:fp
//options_all:--microsoft_version 1928 --ms_c++latest -w
//remark:[6.2] Coroutines in lambda expressions
// 1/15/21  [EDGcpfe/23728]
//
// Coroutines in lambda expressions
//
// The implementation of the restriction that coroutines cannot be constexpr
// functions in the front end failed to account for functions that are implicitly
// constexpr if they can be constexpr.  Most notably, this includes lambda
// expressions.
//
// This is now fixed.
#include <coroutine>
void f()
{
  // Spurious "a constexpr function cannot be a coroutine" error
  auto g = []() -> std::generator<int> {
    co_yield 1;
    co_yield 2;
  };
}
