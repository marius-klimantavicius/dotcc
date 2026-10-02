//type:fp
//options:--c++20:--microsoft_version 1920 --c++17
//options_all:--set_flag coroutines

#include <coroutine>

struct A
{
  ~A();

  bool await_ready();
  void await_suspend(std::coroutine_handle<>);
  A await_resume();
};

A operator co_await(const A&)
{
  return {};
}

namespace std
{
  template <typename... Args>
  struct coroutine_traits<A, Args...>
  {
    struct promise_type final
    {
      A get_return_object();
      std::suspend_never initial_suspend();
      std::suspend_never final_suspend() noexcept;
      void unhandled_exception() noexcept;
      void return_void();

      A&& await_transform(A&&);
    };
  };
}

A f()
{
  co_await A{};
}
