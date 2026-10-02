//type: fp
//options:  --c++20
# 1 "SemaCXX/coroutine-uninitialized-warning-crash.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/coroutine-uninitialized-warning-crash.cpp" 2

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
# 3 "SemaCXX/coroutine-uninitialized-warning-crash.cpp" 2

using namespace std;

struct A {
  bool await_ready() { return true; }
  int await_resume() { return 42; }
  template <typename F>
  void await_suspend(F) {}
};


struct coro_t {
  struct promise_type {
    coro_t get_return_object() { return {}; }
    suspend_never initial_suspend() { return {}; }
    suspend_never final_suspend() noexcept { return {}; }
    A yield_value(int) { return {}; }
    void return_void() {}
    static void unhandled_exception() {}
  };
};

coro_t f(int n) {
  if (n == 0)
    co_return;
  co_yield 42;
  int x = co_await A{};
}

template <class Await>
coro_t g(int n) {
  if (n == 0)
    co_return;
  co_yield 42;
  int x = co_await Await{};
}

int main() {
  f(0);
  g<A>(0);
}
