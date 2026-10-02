//type:fn
//options_all:--microsoft_version 1900 --set_flag coroutines -tused

namespace std {
  namespace experimental {

    template<typename T1, typename... Ts> struct first_type {
      using type = T1;
    };

    template<typename... Ts> struct coroutine_traits {
      using promise_type = typename first_type<Ts...>::type::promise_type;
    };

    template<typename PromiseT = void> struct coroutine_handle {};

    struct suspend_always {
      bool await_ready() const noexcept { return false; }
      template<typename T>
      void await_suspend(coroutine_handle<T>) const noexcept { return; }
      void await_resume() const noexcept { return; }
    };

    template<class T, int N = 1> struct task {
      struct promise_type {
        void return_value(T);
        auto initial_suspend() { return suspend_always{}; }
        auto final_suspend() noexcept { return suspend_always{}; }
        void unhandled_exception();
        auto get_return_object() { return task<T, N>{}; }
      };
    };
    template<class T, int... N> struct generator {
      struct promise_type {
        auto yield_value(T) { return suspend_always{}; }
        void return_value(T);
        auto initial_suspend() { return suspend_always{}; }
        auto final_suspend() noexcept { return suspend_always{}; }
        void unhandled_exception();
        auto get_return_object() { return generator<T, N...>{}; }
      };
    };
  }
}

using std::experimental::coroutine_handle;
using std::experimental::task;
using std::experimental::generator;

struct A {
  bool await_ready();
  void* await_suspend(coroutine_handle<task<int>::promise_type>);
  bool await_resume();
};

int g(int);
A h();

generator<int> f(int n, int m) {
  if (n>m) {
    co_yield n;
    co_yield f(n-1, m+1);
    co_yield m;
  }
  co_return;
}

