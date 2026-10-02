//type: fp
//options:  --c++20
# 1 "SemaCXX/coroutine-unreachable-warning.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/coroutine-unreachable-warning.cpp" 2


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
# 4 "SemaCXX/coroutine-unreachable-warning.cpp" 2

extern void abort(void) __attribute__((__noreturn__));

struct task {
  struct promise_type {
    std::suspend_always initial_suspend();
    std::suspend_always final_suspend() noexcept;
    void return_void();
    std::suspend_always yield_value(int) { return {}; }
    task get_return_object();
    void unhandled_exception();

    struct Awaiter {
      bool await_ready();
      void await_suspend(auto);
      int await_resume();
    };
    auto await_transform(const int& x) { return Awaiter{}; }
  };
};

task test1() {
  abort();
  co_yield 1;
}

task test2() {
  abort();
  1;
  co_yield 1;
}

task test3() {
  abort();
  co_return;
}

task test4() {
  abort();
  1;
  co_return;
}

task test5() {
  abort();
  co_await 1;
}

task test6() {
  abort();
  1;
  co_await 3;
}

task test7() {

  co_await 1;
  abort();
  co_await 2;
}

task test8() {

  abort();
  co_return;
  1 + 1;
}

task test9() {
  abort();

  int x = co_await 1;
}
