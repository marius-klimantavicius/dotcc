//type:fp
//options_all:--c++20
//remark:[6.3] Assertion failure with nested co_yield
// 6/23/21  [EDGcpfe/23366]
//
// Assertion failure with nested co_yield
//
// The front end previously aborted with an assertion failure in
// i_copy_expr_tree when the source contains a nested co_yield expression.
// This is now fixed.
#include <coroutine>
template <typename T> struct my_generator {
  struct promise_type {
    my_generator<T> get_return_object() { return {}; }
    std::suspend_never initial_suspend() { return {}; }
    std::suspend_never final_suspend() noexcept { return {}; }
    void unhandled_exception() {}
    auto yield_value(T value) {
      struct awaiter {
        bool await_ready() const noexcept { return true; }
        void await_suspend(std::coroutine_handle<>) const noexcept {}
        int await_resume() const noexcept { return 0; }
      };
      return awaiter{};
    }
    void return_void() {}
  };
};
my_generator<int> f() {
  co_yield 3, co_yield 4;  // Previously aborted
}
