//type: fn
//options:  --c++20
# 1 "SemaCXX/coroutine-mixed-exp-namespace.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 427 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/coroutine-mixed-exp-namespace.cpp" 2




# 1 "SemaCXX/Inputs/std-coroutine-exp-namespace.h" 1




namespace std {
namespace experimental {
template <class Ret, typename... T>
struct coroutine_traits { using promise_type = typename Ret::promise_type; };

template <class Promise = void>
struct coroutine_handle {
  static coroutine_handle from_address(void *) noexcept;
};
template <>
struct coroutine_handle<void> {
  template <class PromiseType>
  coroutine_handle(coroutine_handle<PromiseType>) noexcept;
  static coroutine_handle from_address(void *);
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
}
# 6 "SemaCXX/coroutine-mixed-exp-namespace.cpp" 2
# 1 "SemaCXX/Inputs/std-coroutine.h" 1




namespace std {

template <class Ret, typename... T>
struct coroutine_traits { using promise_type = typename Ret::promise_type; };

template <class Promise = void>
struct coroutine_handle {
  static coroutine_handle from_address(void *) noexcept;
};
template <>
struct coroutine_handle<void> {
  template <class PromiseType>
  coroutine_handle(coroutine_handle<PromiseType>) noexcept;
  static coroutine_handle from_address(void *);
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
# 7 "SemaCXX/coroutine-mixed-exp-namespace.cpp" 2

struct my_awaitable {
  bool await_ready() noexcept;
  void await_suspend(std::experimental::coroutine_handle<> coro) noexcept;
  void await_resume() noexcept;
};

struct promise_void {
  void get_return_object();
  my_awaitable initial_suspend();
  my_awaitable final_suspend() noexcept;
  void return_void();
  void unhandled_exception();
};

template <>
struct std::coroutine_traits<void> { using promise_type = promise_void; };

void test() {
  co_return;



}
