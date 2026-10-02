//type: fn
//options:  --c++20
# 1 "SemaCXX/coro-return-type-and-wrapper.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/coro-return-type-and-wrapper.cpp" 2

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
# 3 "SemaCXX/coro-return-type-and-wrapper.cpp" 2

using std::suspend_always;
using std::suspend_never;


namespace std {
  struct nothrow_t {};
  constexpr nothrow_t nothrow = {};
}

using SizeT = decltype(sizeof(int));

void* operator new(SizeT __sz, const std::nothrow_t&) noexcept;

template <typename T> struct [[clang::coro_return_type]] Gen {
  struct promise_type {
    Gen<T> get_return_object() {
      return {};
    }
    static Gen<T> get_return_object_on_allocation_failure() {
      return {};
    }
    suspend_always initial_suspend();
    suspend_always final_suspend() noexcept;
    void unhandled_exception();
    void return_value(T t);

    template <typename U>
    auto await_transform(const Gen<U> &) {
      struct awaitable {
        bool await_ready() noexcept { return false; }
        void await_suspend(std::coroutine_handle<>) noexcept {}
        U await_resume() noexcept { return {}; }
      };
      return awaitable{};
    }
  };
};

Gen<int> foo_coro(int b);
Gen<int> foo_coro(int b) { co_return b; }

[[clang::coro_wrapper]] Gen<int> marked_wrapper1(int b) { return foo_coro(b); }


Gen<int> non_marked_wrapper(int b) { return foo_coro(b); }

namespace using_decl {
template <typename T> using Co = Gen<T>;

[[clang::coro_wrapper]] Co<int> marked_wrapper1(int b) { return foo_coro(b); }


Co<int> non_marked_wrapper(int b) { return foo_coro(b); }
}

namespace lambdas {

void foo() {
  auto coro_lambda = []() -> Gen<int> {
    co_return 1;
  };

  auto not_allowed_wrapper = []() -> Gen<int> {
    return foo_coro(1);
  };
  auto allowed_wrapper = [] [[clang::coro_wrapper]] () -> Gen<int> {
    return foo_coro(1);
  };
}

Gen<int> coro_containing_lambda() {

  auto wrapper_lambda = []() -> Gen<int> {
    return foo_coro(1);
  };
  co_return co_await wrapper_lambda();
}
}

namespace std_function {
namespace std {
template <typename> class function;

template <typename ReturnValue, typename... Args>
class function<ReturnValue(Args...)> {
public:
  template <typename T> function &operator=(T) {}
  template <typename T> function(T) {}

  ReturnValue operator()(Args... args) const {
    return callable_->Invoke(args...);
  }

private:
  class Callable {
  public:

    ReturnValue Invoke(Args...) const { return {}; }
  };
  Callable* callable_;
};
}

void use_std_function() {
  std::function<int(bool)> foo = [](bool b) { return b ? 1 : 2; };

  std::function<Gen<int>(bool)> test1 = [](bool b) {
    return foo_coro(b);
  };
  std::function<Gen<int>(bool)> test2 = [](bool) -> Gen<int> {
    co_return 1;
  };
  std::function<Gen<int>(bool)> test3 = foo_coro;

  foo(true);
  test1(true);
  test2(true);
  test3(true);
}
}


class [[clang::coro_return_type]] Task{};
struct my_promise_type {
  Task get_return_object() {
    return {};
  }
  suspend_always initial_suspend();
  suspend_always final_suspend() noexcept;
  void unhandled_exception();
};
namespace std {
template<> class coroutine_traits<Task, int> {
    using promise_type = my_promise_type;
};
}

Task foo(int) { return Task{}; }
