//type:fp
//options_all:--microsoft_version=1925 --ms_c++17 --set_flag coroutines
//remark:[6.1] Abort in coroutines when awaiting an operand with a non-trivial destructor
// 1/6/20   [EDGcpfe/22147]
//
// Abort in coroutines when awaiting an operand with a non-trivial destructor
//
// The operand to a (possibly generated) co_await expression produces an
// auxiliary expression "e" for the implicit e.await_ready, e.await_suspend, and
// e.await_resume function calls.  If "e" contains an object destruction, the
// front end would abort in update_last_processed_dynamic_init.
//
// This is now fixed.
namespace std {
  template<typename R, typename ...Args> struct coroutine_traits {
    using promise_type = R::promise_type;
  };
  template <typename = void> struct coroutine_handle {};
}
struct A {
  struct promise_type
  {
    A get_return_object();
    A initial_suspend();
    A final_suspend() noexcept;
    void unhandled_exception() noexcept;
    void return_void();
    A await_transform(A);
  };
  ~A();
  bool await_ready() noexcept;
  template<typename T> void await_suspend(std::coroutine_handle<T>) noexcept;
  A await_resume() noexcept;
};
A f()
{
  co_await A{}; // Previously triggered an abort, now accepted
}
