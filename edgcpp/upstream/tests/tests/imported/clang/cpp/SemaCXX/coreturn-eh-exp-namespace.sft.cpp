//type: fp
//options:  --c++14 --exceptions
# 1 "SemaCXX/coreturn-eh-exp-namespace.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 397 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/coreturn-eh-exp-namespace.cpp" 2


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
# 4 "SemaCXX/coreturn-eh-exp-namespace.cpp" 2

using std::experimental::suspend_always;
using std::experimental::suspend_never;

struct awaitable {
  bool await_ready();
  void await_suspend(std::experimental::coroutine_handle<>);
  void await_resume();
} a;

struct object {
  ~object() {}
};

struct promise_void_return_value {
  void get_return_object();
  suspend_always initial_suspend();
  suspend_always final_suspend() noexcept;
  void unhandled_exception();
  void return_value(object);
};

struct VoidTagReturnValue {
  struct promise_type {
    VoidTagReturnValue get_return_object();
    suspend_always initial_suspend();
    suspend_always final_suspend() noexcept;
    void unhandled_exception();
    void return_value(object);
  };
};

template <typename T1>
struct std::experimental::coroutine_traits<void, T1> { using promise_type = promise_void_return_value; };

VoidTagReturnValue test() {
  object x = {};
  try {
    co_return {};

  } catch (...) {
    throw;
  }
}
