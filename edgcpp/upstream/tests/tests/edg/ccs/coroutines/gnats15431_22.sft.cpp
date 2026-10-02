//type:fp
//options:--microsoft_version 1900
//options_all:--set_flag coroutines -tused

#include <coroutine>

namespace std {
  struct my_suspend : suspend_always {
    int await_resume() noexcept;
  };
  
  template <>
    struct generator<int> {
    struct promise_type {
      auto get_return_object() { return generator{}; }
      auto initial_suspend() { return my_suspend{}; }
      auto final_suspend() noexcept { return my_suspend{}; }
      void unhandled_exception() {}
      auto yield_value(int) { return my_suspend{}; }
      void return_value(int) {}
    };
    ~generator() {}
  };
}

std::generator<int> f()
{
  auto i = co_yield 1;
  co_return i;
}

