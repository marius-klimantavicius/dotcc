//type:fp
//options:--c++20:--c++20 --gn 130200:--c++20 --clang_version 170000:--ms_c++20 --microsoft_version 1936
//options_all:--no_defer_parse_function_templates -tused
#include <coroutine>

namespace minimal
{
  //#include <coroutine>
  struct A {
    struct promise_type {
      A get_return_object();
      std::suspend_never initial_suspend();
      std::suspend_never final_suspend() noexcept;
      void unhandled_exception();
      void return_void() {}
    };
  };
  auto l = [] (auto) -> A {
    co_await std::suspend_always{};
  };
}

struct X {
  struct promise_type {
    X get_return_object();
    auto initial_suspend() { return std::suspend_always{}; }
    auto final_suspend() noexcept { return std::suspend_always{}; }
    void unhandled_exception();
    auto yield_value(int) { return std::suspend_always{}; }
    void return_void() {}
  };
};

namespace instantiated
{
  template<typename T>
  X implicit_return(T)
  {
    co_await std::suspend_always{};
    co_yield 1;
  }

  template<typename T>
  X explicit_return(T)
  {
    co_await std::suspend_always{};
    co_yield 1;
    co_return;
  }

  void f()
  {
    implicit_return(0);
    explicit_return(0);
  }
}

namespace dependent_types_dependent_promise
{
  template<typename T, typename U = std::suspend_always>
  X implicit_return(T)
  {
    co_await U{};
    co_yield 1;
  }

  template<typename T, typename U = std::suspend_always>
  X explicit_return(T)
  {
    co_await U{};
    co_yield 1;
    co_return;
  }

  void f()
  {
    implicit_return(0);
    explicit_return(0);
  }
}

namespace dependent_types_non_dependent_promise
{
  template<typename U = std::suspend_always>
  X implicit_return(int)
  {
    co_await U{};
    co_yield 1;
  }

  template<typename U = std::suspend_always>
  X explicit_return(int)
  {
    co_await U{};
    co_yield 1;
    co_return;
  }

  void f()
  {
    implicit_return(0);
    explicit_return(0);
  }
}

namespace invalid_non_dependent_type
{
  void f();

  template<typename T>
  X implicit_return(T)
  {
    co_await f;
    co_yield f;
  }

  template<typename T>
  X explicit_return(T)
  {
    co_await f;
    co_yield f;
    co_return;
  }
}
