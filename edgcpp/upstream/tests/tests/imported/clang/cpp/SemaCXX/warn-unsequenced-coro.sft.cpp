//type: fp
//options:  --c++20
# 1 "SemaCXX/warn-unsequenced-coro.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-unsequenced-coro.cpp" 2




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
# 6 "SemaCXX/warn-unsequenced-coro.cpp" 2

typedef long int ptrdiff_t;

using namespace std;

template<class T>
struct Task {
    struct promise_type {
        Task<T> get_return_object() noexcept;
        suspend_always initial_suspend() noexcept;
        suspend_always final_suspend() noexcept;
        void return_value(T);
        void unhandled_exception();
        auto yield_value(Task<T>) noexcept { return final_suspend(); }
    };
    bool await_ready() noexcept { return false; }
    void await_suspend(coroutine_handle<>) noexcept {}
    T await_resume();
};

template<>
struct Task<void> {
    struct promise_type {
        Task<void> get_return_object() noexcept;
        suspend_always initial_suspend() noexcept;
        suspend_always final_suspend() noexcept;
        void return_void() noexcept;
        void unhandled_exception() noexcept;
        auto yield_value(Task<void>) noexcept { return final_suspend(); }
    };
    bool await_ready() noexcept { return false; }
    void await_suspend(coroutine_handle<>) noexcept {}
    void await_resume() noexcept {}
};

template <typename T>
class generator
{
  struct Promise
  {
    auto get_return_object() { return generator{*this}; }
    auto initial_suspend() { return suspend_never{}; }
    auto final_suspend() noexcept { return suspend_always{}; }
    void unhandled_exception() {}
    void return_void() {}

    auto yield_value(T value)
    {
      value_ = std::move(value);
      return suspend_always{};
    }

    T value_;
  };

  using Handle = coroutine_handle<Promise>;

  struct sentinel{};
  struct iterator
  {
    using iterator_category = input_iterator_tag;
    using value_type = T;
    using difference_type = ptrdiff_t;
    using reference = T &;
    using const_reference = const T &;
    using pointer = T *;

    iterator &operator++()
    {
      h_.resume();
      return *this;
    }
    const_reference &operator*() const { return h_.promise().value_; }
    bool operator!=(sentinel) { return !h_.done(); }

    Handle h_;
  };

  explicit generator(Promise &p) : h_(Handle::from_promise(p)) {}
  Handle h_;
public:
  using promise_type = Promise;
  auto begin() { return iterator{h_}; }
  auto end() { return sentinel{}; }
};

Task<void> c(int i) {
  co_await (i = 0, std::suspend_always{});
}

generator<int> range(int start, int end)
{
  while (start < end)
    co_yield start++;
}

Task<int> go(int const& val);
Task<int> go1(int x) {
  co_return co_await go(++x);
}
