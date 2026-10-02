//type: fn
//options:  --c++20
# 1 "SemaCXX/addr-label-in-coroutines.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/addr-label-in-coroutines.cpp" 2


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
# 4 "SemaCXX/addr-label-in-coroutines.cpp" 2

struct resumable {
  struct promise_type {
    resumable get_return_object() { return {}; }
    auto initial_suspend() { return std::suspend_always(); }
    auto final_suspend() noexcept { return std::suspend_always(); }
    void unhandled_exception() {}
    void return_void(){};
  };
};

resumable f1(int &out, int *inst) {
    static void* dispatch_table[] = {&&inc,
                                     &&suspend,
                                     &&stop};

inc:
    out++;
    goto *dispatch_table[*inst++];

suspend:
    co_await std::suspend_always{};
    goto *dispatch_table[*inst++];

stop:
    co_return;
}

resumable f2(int &out, int *inst) {
    void* dispatch_table[] = {nullptr, nullptr, nullptr};
    dispatch_table[0] = &&inc;
    dispatch_table[1] = &&suspend;
    dispatch_table[2] = &&stop;

inc:
    out++;
    goto *dispatch_table[*inst++];

suspend:
    co_await std::suspend_always{};
    goto *dispatch_table[*inst++];

stop:
    co_return;
}

resumable f3(int &out, int *inst) {
    void* dispatch_table[] = {nullptr, nullptr, nullptr};
    [&]() -> resumable {
        dispatch_table[0] = &&inc;
        dispatch_table[1] = &&suspend;
        dispatch_table[2] = &&stop;

    inc:
        out++;
        goto *dispatch_table[*inst++];

    suspend:
        co_await std::suspend_always{};
        goto *dispatch_table[*inst++];

    stop:
        co_return;
    }();

    co_return;
}
