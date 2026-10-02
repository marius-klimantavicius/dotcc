//type: fp
//options:  --c++20
# 1 "SemaCXX/coro-lifetimebound.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/coro-lifetimebound.cpp" 2


# 1 "SemaCXX/Inputs/std-coroutine.h" 1




namespace std {

template<typename T> struct remove_reference { typedef T type; };
template<typename T> struct remove_reference<T &> { typedef T type; };
template<typename T> struct remove_reference<T &&> { typedef T type; };

template<typename T>
typename remove_reference<T>::type &&move(T &&t) noexcept;

struct input_iterator_tag {};
struct forward_iterator_tag : public input_iterator_tag {};

template <class Ret, typename... T>
struct coroutine_traits { using promise_type = typename Ret::promise_type; };

template <class Promise = void>
struct coroutine_handle {
  static coroutine_handle from_address(void *) noexcept;
  static coroutine_handle from_promise(Promise &promise);
  constexpr void* address() const noexcept;
};
template <>
struct coroutine_handle<void> {
  template <class PromiseType>
  coroutine_handle(coroutine_handle<PromiseType>) noexcept;
  static coroutine_handle from_address(void *);
  constexpr void* address() const noexcept;
};

struct suspend_always {
  bool await_ready() noexcept { return false; }
  void await_suspend(coroutine_handle<>) noexcept {}
  void await_resume() noexcept {}
};

struct suspend_never {
  bool await_ready() noexcept { return true; }
  void await_suspend(coroutine_handle<>) noexcept {}
  void await_resume() noexcept {}
};

}
# 4 "SemaCXX/coro-lifetimebound.cpp" 2

using std::suspend_always;
using std::suspend_never;

template <typename T> struct [[clang::coro_lifetimebound, clang::coro_return_type]] Co {
  struct promise_type {
    Co<T> get_return_object() {
      return {};
    }
    suspend_always initial_suspend();
    suspend_always final_suspend() noexcept;
    void unhandled_exception();
    void return_value(const T &t);

    template <typename U>
    auto await_transform(const Co<U> &) {
      struct awaitable {
        bool await_ready() noexcept { return false; }
        void await_suspend(std::coroutine_handle<>) noexcept {}
        U await_resume() noexcept { return {}; }
      };
      return awaitable{};
    }
  };
};

Co<int> foo_coro(const int& b) {
  if (b > 0)
    co_return 1;
  co_return 2;
}

int getInt() { return 0; }

Co<int> bar_coro(const int &b, int c) {
  int x = co_await foo_coro(b);
  int y = co_await foo_coro(1);
  int z = co_await foo_coro(getInt());
  auto unsafe1 = foo_coro(1);
  auto unsafe2 = foo_coro(getInt());
  auto safe1 = foo_coro(b);
  auto safe2 = foo_coro(c);
  co_return co_await foo_coro(co_await foo_coro(1));
}

[[clang::coro_wrapper]] Co<int> plain_return_co(int b) {
  return foo_coro(b);
}

[[clang::coro_wrapper]] Co<int> safe_forwarding(const int& b) {
  return foo_coro(b);
}

[[clang::coro_wrapper]] Co<int> unsafe_wrapper(int b) {
  return safe_forwarding(b);
}

[[clang::coro_wrapper]] Co<int> complex_plain_return(int b) {
  return b > 0
      ? foo_coro(1)
      : bar_coro(0, 1);
}




namespace lambdas {
void lambdas() {
  auto unsafe_lambda = [] [[clang::coro_wrapper]] (int b) {
    return foo_coro(b);
  };
  auto coro_lambda = [] (const int&) -> Co<int> {
    co_return 0;
  };
  auto unsafe_coro_lambda = [&] (const int& b) -> Co<int> {
    int x = co_await coro_lambda(b);
    auto safe = coro_lambda(b);
    auto unsafe1 = coro_lambda(1);
    auto unsafe2 = coro_lambda(getInt());
    auto unsafe3 = coro_lambda(co_await coro_lambda(b));
    co_return co_await safe;
  };
  auto safe_lambda = [](int b) -> Co<int> {
    int x = co_await foo_coro(1);
    co_return x + co_await foo_coro(b);
  };
}

Co<int> lambda_captures() {
  int a = 1;

  auto lamb = [a](int x, const int& y) -> Co<int> {
    co_return x + y + a;
  }(1, a);

  auto no_capture = []() -> Co<int> { co_return 1; }();
  auto bad_no_capture = [](const int& a) -> Co<int> { co_return a; }(1);

  int res = co_await [a](int x, const int& y) -> Co<int> {
    co_return x + y + a;
  }(1, a);

  auto lamb2 = [a]() -> Co<int> { co_return a; };
  auto on_stack = lamb2();
  auto res2 = co_await on_stack;
  co_return 1;
}
}




namespace member_coroutines{
struct S {
  Co<int> member(const int& a) { co_return a; }
};

Co<int> use() {
  S s;
  int a = 1;
  auto test1 = s.member(1);
  auto test2 = s.member(a);
  auto test3 = S{}.member(a);
  co_return 1;
}

[[clang::coro_wrapper]] Co<int> wrapper(const int& a) {
  S s;
  return s.member(a);
}
}




namespace by_value {
Co<int> value_coro(int b) { co_return co_await foo_coro(b); }
[[clang::coro_wrapper]] Co<int> wrapper1(int b) { return value_coro(b); }
[[clang::coro_wrapper]] Co<int> wrapper2(const int& b) { return value_coro(b); }
}




namespace not_a_crt {
template <typename T> struct [[clang::coro_lifetimebound]] CoNoCRT {
  struct promise_type {
    CoNoCRT<T> get_return_object() {
      return {};
    }
    suspend_always initial_suspend();
    suspend_always final_suspend() noexcept;
    void unhandled_exception();
    void return_value(const T &t);
  };
};

CoNoCRT<int> foo_coro(const int& a) { co_return a; }
CoNoCRT<int> bar(int a) {
  auto x = foo_coro(a);
  co_return 1;
}
}




namespace disable_lifetimebound {
Co<int> foo(int x) { co_return x; }

[[clang::coro_wrapper, clang::coro_disable_lifetimebound]]
Co<int> foo_wrapper(const int& x) { return foo(x); }

[[clang::coro_wrapper]] Co<int> caller() {

  return foo_wrapper(1);
}

struct S{
[[clang::coro_wrapper, clang::coro_disable_lifetimebound]]
Co<int> member(const int& x) { return foo(x); }
};

Co<int> use() {
  S s;
  int a = 1;
  auto test1 = s.member(1);
  auto test2 = S{}.member(a);
  co_return 1;
}

[[clang::coro_wrapper]] Co<int> return_stack_addr(const int& a) {
  S s;
  return s.member(a);
}
}
