//type: fp
//options:  --c++20 --exceptions
# 1 "SemaCXX/warn-throw-out-noexcept-coro.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-throw-out-noexcept-coro.cpp" 2


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
# 4 "SemaCXX/warn-throw-out-noexcept-coro.cpp" 2



template <typename T>
struct promise;

template <typename T>
struct task {
    using promise_type = promise<T>;

    explicit task(promise_type& p) { throw 1; p.return_val = this; }

    T value;
};

template <typename T>
struct promise {
    task<T> get_return_object() { return task{*this}; }

    std::suspend_never initial_suspend() const noexcept { return {}; }

    std::suspend_never final_suspend() const noexcept { return {}; }

    template <typename U>
    void return_value(U&& val) { return_val->value = static_cast<U&&>(val); }

    void unhandled_exception() { throw 1; }

    task<T>* return_val;
};

task<int> a_ShouldNotDiag(const int a, const int b) {
  if (b == 0)
    throw b;

  co_return a / b;
}

task<int> b_ShouldNotDiag(const int a, const int b) noexcept {
  if (b == 0)
    throw b;

  co_return a / b;
}

const auto c_ShouldNotDiag = [](const int a, const int b) -> task<int> {
  if (b == 0)
    throw b;

  co_return a / b;
};

const auto d_ShouldNotDiag = [](const int a, const int b) noexcept -> task<int> {
  if (b == 0)
    throw b;

  co_return a / b;
};
