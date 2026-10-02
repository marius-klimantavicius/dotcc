//type:fp
//options_all:--set_flag coroutines --c++20 --il_display --no_il_lowering
//require:NEED_IL_DISPLAY 1
//require:DO_IL_LOWERING 1
//filter:egrep -A3 '(eok_not|stmk_goto|stmk_label)' | edg-normalize-test-output --il

#include <coroutine>

struct R;

struct my_promise_type
{
  R get_return_object() noexcept;
  auto initial_suspend() noexcept { return std::suspend_never{}; }
  auto final_suspend() noexcept { return std::suspend_never{}; }
  void unhandled_exception() noexcept { }
  void return_void() noexcept { }
};

struct R
{
  using promise_type = my_promise_type;
};

struct A
{
  ~A();
};

R foo()
{
  A a;
  co_return;
}
