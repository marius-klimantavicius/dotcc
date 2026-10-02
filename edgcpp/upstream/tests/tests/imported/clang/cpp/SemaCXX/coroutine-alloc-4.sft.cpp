//type: fn
//options:  --c++20
# 1 "SemaCXX/coroutine-alloc-4.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/coroutine-alloc-4.cpp" 2



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
# 5 "SemaCXX/coroutine-alloc-4.cpp" 2

namespace std {
    typedef long unsigned int size_t;
    enum class align_val_t : size_t {};
}

struct task {
  struct promise_type {
    auto initial_suspend() { return std::suspend_always{}; }
    auto final_suspend() noexcept { return std::suspend_always{}; }
    auto get_return_object() { return task{}; }
    void unhandled_exception() {}
    void return_value(int) {}
    void *operator new(std::size_t);
  };
};

task f() {
    co_return 43;
}

struct task2 {
  struct promise_type {
    auto initial_suspend() { return std::suspend_always{}; }
    auto final_suspend() noexcept { return std::suspend_always{}; }
    auto get_return_object() { return task2{}; }
    void unhandled_exception() {}
    void return_value(int) {}
    void *operator new(std::size_t, std::align_val_t);
  };
};


task2 f1() {
    co_return 43;
}

struct task3 {
  struct promise_type {
    auto initial_suspend() { return std::suspend_always{}; }
    auto final_suspend() noexcept { return std::suspend_always{}; }
    auto get_return_object() { return task3{}; }
    void unhandled_exception() {}
    void return_value(int) {}
    void *operator new(std::size_t, std::align_val_t) noexcept;
    void *operator new(std::size_t) noexcept;
    static auto get_return_object_on_allocation_failure() { return task3{}; }
  };
};


task3 f2() {
    co_return 43;
}

struct task4 {
  struct promise_type {
    auto initial_suspend() { return std::suspend_always{}; }
    auto final_suspend() noexcept { return std::suspend_always{}; }
    auto get_return_object() { return task4{}; }
    void unhandled_exception() {}
    void return_value(int) {}
    void *operator new(std::size_t, std::align_val_t, int, double, int) noexcept;
  };
};


task4 f3(int, double, int) {
    co_return 43;
}

struct task5 {
  struct promise_type {
    auto initial_suspend() { return std::suspend_always{}; }
    auto final_suspend() noexcept { return std::suspend_always{}; }
    auto get_return_object() { return task5{}; }
    void unhandled_exception() {}
    void return_value(int) {}
  };
};



task5 f4() {
    co_return 43;
}

namespace std {
  struct nothrow_t {};
  constexpr nothrow_t nothrow = {};
}

struct task6 {
  struct promise_type {
    auto initial_suspend() { return std::suspend_always{}; }
    auto final_suspend() noexcept { return std::suspend_always{}; }
    auto get_return_object() { return task6{}; }
    void unhandled_exception() {}
    void return_value(int) {}
    static task6 get_return_object_on_allocation_failure() { return task6{}; }
  };
};

task6 f5() {
    co_return 43;
}

void *operator new(std::size_t, std::align_val_t, std::nothrow_t) noexcept;

task6 f6() {
    co_return 43;
}
