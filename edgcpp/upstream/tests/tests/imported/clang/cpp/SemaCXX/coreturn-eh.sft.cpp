//type: fp
//options:  --c++20 --exceptions
# 1 "SemaCXX/coreturn-eh.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/coreturn-eh.cpp" 2



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
# 5 "SemaCXX/coreturn-eh.cpp" 2

using std::suspend_always;
using std::suspend_never;

struct awaitable {
  bool await_ready();
  void await_suspend(std::coroutine_handle<>);
  void await_resume();
} a;

struct object { ~object() {} };

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
struct std::coroutine_traits<void, T1> { using promise_type = promise_void_return_value; };

VoidTagReturnValue test() {
  object x = {};
  try {
    co_return {};
  } catch (...) {
    throw;
  }
}
