//type: fp
//options:  --c++20
# 1 "SemaCXX/coroutine-no-move-ctor.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/coroutine-no-move-ctor.cpp" 2



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
# 5 "SemaCXX/coroutine-no-move-ctor.cpp" 2

class invoker {
public:
  class invoker_promise {
  public:
    invoker get_return_object() { return invoker{}; }
    auto initial_suspend() { return std::suspend_never{}; }
    auto final_suspend() noexcept { return std::suspend_never{}; }
    void return_void() {}
    void unhandled_exception() {}
  };
  using promise_type = invoker_promise;
  invoker() {}
  invoker(const invoker &) = delete;
  invoker &operator=(const invoker &) = delete;
  invoker(invoker &&) = delete;
  invoker &operator=(invoker &&) = delete;
};

invoker f() {
  co_return;
}
