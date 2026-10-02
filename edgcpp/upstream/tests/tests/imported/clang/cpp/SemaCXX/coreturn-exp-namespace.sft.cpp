//type: fp
//options:  --c++14
# 1 "SemaCXX/coreturn-exp-namespace.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 397 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/coreturn-exp-namespace.cpp" 2

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
# 3 "SemaCXX/coreturn-exp-namespace.cpp" 2

using std::experimental::suspend_always;
using std::experimental::suspend_never;

struct awaitable {
  bool await_ready();
  void await_suspend(std::experimental::coroutine_handle<>);
  void await_resume();
} a;

struct promise_void {
  void get_return_object();
  suspend_always initial_suspend();
  suspend_always final_suspend() noexcept;
  void return_void();
  void unhandled_exception();
};

struct promise_void_return_value {
  void get_return_object();
  suspend_always initial_suspend();
  suspend_always final_suspend() noexcept;
  void unhandled_exception();
  void return_value(int);
};

struct VoidTagNoReturn {
  struct promise_type {
    VoidTagNoReturn get_return_object();
    suspend_always initial_suspend();
    suspend_always final_suspend() noexcept;
    void unhandled_exception();
  };
};

struct VoidTagReturnValue {
  struct promise_type {
    VoidTagReturnValue get_return_object();
    suspend_always initial_suspend();
    suspend_always final_suspend() noexcept;
    void unhandled_exception();
    void return_value(int);
  };
};

struct VoidTagReturnVoid {
  struct promise_type {
    VoidTagReturnVoid get_return_object();
    suspend_always initial_suspend();
    suspend_always final_suspend() noexcept;
    void unhandled_exception();
    void return_void();
  };
};

struct promise_float {
  float get_return_object();
  suspend_always initial_suspend();
  suspend_always final_suspend() noexcept;
  void return_void();
  void unhandled_exception();
};

struct promise_int {
  int get_return_object();
  suspend_always initial_suspend();
  suspend_always final_suspend() noexcept;
  void return_value(int);
  void unhandled_exception();
};

template <>
struct std::experimental::coroutine_traits<void> { using promise_type = promise_void; };

template <typename T1>
struct std::experimental::coroutine_traits<void, T1> { using promise_type = promise_void_return_value; };

template <typename... T>
struct std::experimental::coroutine_traits<float, T...> { using promise_type = promise_float; };

template <typename... T>
struct std::experimental::coroutine_traits<int, T...> { using promise_type = promise_int; };

void test0() { co_await a; }

float test1() { co_await a; }

int test2() {
  co_await a;
}

int test2a(bool b) {
  if (b)
    co_return 42;
}

int test3() {
  co_await a;
b:
  goto b;
}

int test4() {
  co_return 42;
}

void test5(int) {
  co_await a;
}

void test6(int x) {
  if (x)
    co_return 42;
}

void test7(int y) {
  if (y)
    co_return 42;
  else
    co_return 101;
}

VoidTagReturnVoid test8() {
  co_await a;
}

VoidTagReturnVoid test9(bool b) {
  if (b)
    co_return;
}

VoidTagReturnValue test10() {
  co_await a;
}

VoidTagReturnValue test11(bool b) {
  if (b)
    co_return 42;
}
