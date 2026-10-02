//type: fn
//options:  --c++26 --exceptions
# 1 "SemaCXX/type-aware-coroutines.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 499 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/type-aware-coroutines.cpp" 2



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
# 5 "SemaCXX/type-aware-coroutines.cpp" 2

namespace std {
   template <typename T> struct type_identity {
   typedef T type;
   };
   typedef long unsigned int size_t;
   enum class align_val_t : size_t {};
}

struct Allocator {};

struct resumable {
  struct promise_type {
    void *operator new(std::type_identity<promise_type>, std::size_t sz, std::align_val_t, int);
    void *operator new(std::type_identity<promise_type>, std::size_t sz, std::align_val_t, float);
    void operator delete(std::type_identity<promise_type>, void *, std::size_t sz, std::align_val_t);
    template <typename T> void operator delete(std::type_identity<T>, void *, std::size_t sz, std::align_val_t) = delete;

    resumable get_return_object() { return {}; }
    auto initial_suspend() { return std::suspend_always(); }
    auto final_suspend() noexcept { return std::suspend_always(); }
    void unhandled_exception() {}
    void return_void(){};
    std::suspend_always yield_value(int i);
  };
};

struct resumable2 {
  struct promise_type {
    template <typename... Args> void *operator new(std::type_identity<promise_type>, std::size_t sz, std::align_val_t, Args...);
    void operator delete(std::type_identity<promise_type>, void *, std::size_t sz, std::align_val_t);

    resumable2 get_return_object() { return {}; }
    auto initial_suspend() { return std::suspend_always(); }
    auto final_suspend() noexcept { return std::suspend_always(); }
    void unhandled_exception() {}
    void return_void(){};
    std::suspend_always yield_value(int i);
  };
};


struct resumable3 {
  struct promise_type {


    void *operator new(std::size_t sz, float);
    void *operator new(std::type_identity<promise_type>, std::size_t sz, std::align_val_t, float);
    void operator delete(void *);

    resumable3 get_return_object() { return {}; }
    auto initial_suspend() { return std::suspend_always(); }
    auto final_suspend() noexcept { return std::suspend_always(); }
    void unhandled_exception() {}
    void return_void(){};
    std::suspend_always yield_value(int i);
  };
};
struct resumable4 {
  struct promise_type {


    void *operator new(std::size_t sz, float);
    template <typename T> void operator delete(std::type_identity<T>, void *, std::size_t, std::align_val_t);

    resumable4 get_return_object() { return {}; }
    auto initial_suspend() { return std::suspend_always(); }
    auto final_suspend() noexcept { return std::suspend_always(); }
    void unhandled_exception() {}
    void return_void(){};
    std::suspend_always yield_value(int i);
  };
};
struct resumable5 {
  struct promise_type {


    void *operator new(std::size_t sz, float);
    void operator delete(void *);
    template <typename T> void operator delete(std::type_identity<T>, void *, std::size_t, std::align_val_t);

    resumable5 get_return_object() { return {}; }
    auto initial_suspend() { return std::suspend_always(); }
    auto final_suspend() noexcept { return std::suspend_always(); }
    void unhandled_exception() {}
    void return_void(){};
    std::suspend_always yield_value(int i);
  };
};

resumable f1(int) {




  co_return;
}

resumable f2(float) {




  co_return;
}

resumable2 f3(int, float, const char*, Allocator) {



  co_yield 1;
  co_return;
}

resumable f4(int n = 10) {




  for (int i = 0; i < n; i++)
    co_yield i;
}
resumable3 f5(float) {


  co_return;
}

resumable4 f6(float) {




  co_return;
}

resumable5 f7(float) {


  co_return;
}
