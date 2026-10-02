//type: fn
//options:  --c++23
# 1 "SemaCXX/cxx2b-deducing-this-coro.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 499 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/cxx2b-deducing-this-coro.cpp" 2


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
# 4 "SemaCXX/cxx2b-deducing-this-coro.cpp" 2

struct S;
template <typename T>
class coro_test {
public:
    struct promise_type;
    using handle = std::coroutine_handle<promise_type>;
 struct promise_type {
        promise_type(const promise_type&) = delete;
        promise_type(T);
        coro_test get_return_object();
        std::suspend_never initial_suspend();
     std::suspend_never final_suspend() noexcept;
     void return_void();
        void unhandled_exception();


        template<typename Arg, typename... Args>
        void* operator new(decltype(0zu) sz, Arg&&, Args&... args) {
            static_assert(!__is_same(__decay(Arg), S), "Ok");
        }

    };
private:
 handle h;
};


template <typename Ret, typename... P>
struct std::coroutine_traits<coro_test<S&>, Ret, P...> {
  using promise_type = coro_test<S&>::promise_type;
  static_assert(!__is_same(Ret, S&), "Ok");
};


struct S {

    coro_test<S&> ok(this S&, int) {
        co_return;
    }

    coro_test<const S&> ok2(this const S&) {
        co_return;
    }

    coro_test<int> ko(this const S&) {




        co_return;
    }

};
