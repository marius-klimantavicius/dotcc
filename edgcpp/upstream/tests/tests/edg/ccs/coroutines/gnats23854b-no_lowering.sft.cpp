//type:cp
//options_all:--il_display -w --c++20 --set_flag coroutines --no_il_lowering
//filter:grep 'BAD' | edg-normalize-test-output --il
//require:DO_IL_LOWERING 1

namespace std {
  typedef decltype(nullptr) nullptr_t;

  template<class Promise = void>
  struct coroutine_handle;

  template <class R, class... ArgTypes>
  struct coroutine_traits {
    using promise_type = typename R::promise_type;
  };

  template<>
  struct coroutine_handle<void>
  {
    constexpr coroutine_handle() noexcept
    : ptr(nullptr)
    {}

    constexpr coroutine_handle(nullptr_t) noexcept
    : ptr(nullptr)
    {}

    coroutine_handle& operator=(nullptr_t) noexcept {
      ptr = nullptr;
      return *this;
    }

    constexpr void* address() const noexcept { return ptr; }
    constexpr static coroutine_handle from_address(void* addr) {
      return *(coroutine_handle<void>*)(addr);
    }

    constexpr explicit operator bool() const noexcept {
      return address() != nullptr;
    }
    bool done() const = delete;

    void operator()() const = delete;
    void resume() const = delete;
    void destroy() const = delete;

  protected:
    void* ptr;
  };

  template<class Promise>
  struct coroutine_handle : coroutine_handle<>
  {
    using coroutine_handle<>::coroutine_handle;

    static coroutine_handle from_promise(Promise&) = delete;

    constexpr static coroutine_handle from_address(void* addr) {
      return *(coroutine_handle<Promise>*)(addr);
    }

    Promise& promise() const = delete;
  };
}

struct awaitable_type {
  bool await_ready() const noexcept;
  void await_suspend(std::coroutine_handle<>) const noexcept;
  bool await_resume() const noexcept;
};

struct coroutine_return_type {
  struct promise_type {
    promise_type();
    coroutine_return_type get_return_object();
    awaitable_type initial_suspend();
    awaitable_type final_suspend() noexcept;
    void return_void();
    void unhandled_exception();
  };
};

struct S {
  coroutine_return_type coro(int p1) {
    this;
    p1;
    co_return;
  }
};
