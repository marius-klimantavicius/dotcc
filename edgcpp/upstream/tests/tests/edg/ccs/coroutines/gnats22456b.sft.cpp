//type:fp
//options::--clang_version 80000
//options_all:--c++17 --set_flag=coroutines -tused

namespace std {
  template<typename = void> class coroutine_handle;
  template<> class coroutine_handle<>{};
  template <typename T> class coroutine_handle : public coroutine_handle<> {};

  struct X {
    bool await_ready() noexcept;
    void await_suspend(coroutine_handle<>) noexcept;
    void await_resume() noexcept;
  };
  struct promise_type {
    X get_return_object();
    X initial_suspend();
    X final_suspend() noexcept;
    void return_void();
    void unhandled_exception();
  };

  template <typename...> struct coroutine_traits {
    using promise_type = promise_type;
  };
} // namespace std
using namespace std;

X func() {
  co_await (X&&)X();
}
