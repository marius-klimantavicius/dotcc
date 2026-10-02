//type:fp
//options:--c++20:--ms_c++20:--c++20 --gnu_version 120100

namespace std
{
  using nullptr_t = decltype(nullptr);

  template<typename...>
  using __void_t = void;

  template<class _Promise = void>
  struct coroutine_handle;

  template<>
  struct coroutine_handle<void>
  {
    constexpr coroutine_handle() noexcept = default;
    constexpr coroutine_handle(nullptr_t) noexcept
    { }

    coroutine_handle& operator=(nullptr_t) noexcept
    {
      __handle_ = nullptr;
      return *this;
    }

    constexpr void* address() const noexcept
    {
      return __handle_;
    }

    static constexpr coroutine_handle from_address(void* __addr) noexcept
    {
      coroutine_handle __tmp;
      __tmp.__handle_ = __addr;
      return __tmp;
    }

    constexpr explicit operator bool() const noexcept
    {
      return __handle_ != nullptr;
    }

    bool done() const
    {
      return false;
    }

    void operator()() const
    {
      resume();
    }

    void resume() const
    { }

    void destroy() const
    { }

    void* __handle_ = nullptr;
  };

  template<class _Promise>
  struct coroutine_handle {
    constexpr coroutine_handle() noexcept = default;
    constexpr coroutine_handle(nullptr_t) noexcept
    { }

    static coroutine_handle from_promise(_Promise& __promise)
    {
      coroutine_handle __tmp;
      return __tmp;
    }

    coroutine_handle& operator=(nullptr_t) noexcept
    {
      __handle_ = nullptr;
      return *this;
    }

    constexpr void* address() const noexcept
    {
      return __handle_;
    }

    static constexpr coroutine_handle from_address(void* __addr) noexcept
    {
      coroutine_handle __tmp;
      __tmp.__handle_ = __addr;
      return __tmp;
    }

    constexpr operator coroutine_handle<>() const noexcept
    {
      return coroutine_handle<>::from_address(address());
    }

    constexpr explicit operator bool() const noexcept
    {
      return __handle_ != nullptr;
    }

    bool done() const
    {
      return false;
    }

    void operator()() const
    {
      resume();
    }

    void resume() const
    { }

    void destroy() const
    { }

    _Promise& promise() const
    {
      return *static_cast<_Promise*>(nullptr);
    }

    void* __handle_ = nullptr;
  };

  template<class _Tp, class = void>
  struct __coroutine_traits_sfinae
  { };

  template<class _Tp>
  struct __coroutine_traits_sfinae<
    _Tp, __void_t<typename _Tp::promise_type> >
  {
    using promise_type = typename _Tp::promise_type;
  };

  template<class _Ret, class... _Args>
  struct coroutine_traits
    : public __coroutine_traits_sfinae<_Ret>
  { };

  struct suspend_never
  {
    constexpr bool await_ready() const noexcept
    { return true; }

    constexpr void await_suspend(coroutine_handle<>) const noexcept
    { }

    constexpr void await_resume() const noexcept
    { }
  };

  struct suspend_always
  {
    constexpr bool await_ready() const noexcept
    { return false; }

    constexpr void await_suspend(coroutine_handle<>) const noexcept
    { }

    constexpr void await_resume() const noexcept
    { }
  };
}

struct awaitable
{
  awaitable() = default;
  awaitable(awaitable const&) = delete;
  awaitable(awaitable &&) = delete;
  bool await_ready() { return true; }
  void await_suspend(std::coroutine_handle<> h) { }
  void await_resume() { }
};

namespace default_ctor
{
  template<typename T>
  struct tmpl_task
  {
    struct C
    {
      C(const char *) { T::do_not_instantiate; }
      ~C() { T::do_not_instantiate; }
    };

    struct promise_type
    {
      promise_type() { T(); }   // expect: expression has no effect
      promise_type(int, const char *) { T(); }
      promise_type(C, const char *) { T(); }
      ~promise_type() { T(); }  // expect: expression has no effect

      tmpl_task get_return_object() { return { }; }
      std::suspend_never initial_suspend() { return { }; }
      std::suspend_never final_suspend() noexcept { return { }; }
      void return_void() { }
      void unhandled_exception() { }
    };
  };

  tmpl_task<int> f(const char *, int)
  {
    co_await awaitable{ };
  }
}

namespace non_default_ctor
{
  template<typename T>
  struct tmpl_task
  {
    struct promise_type
    {
      promise_type() { T(); }
      promise_type(int, int) { T(); } // expect: expression has no effect
      ~promise_type() { T(); }  // expect: expression has no effect

      tmpl_task get_return_object() { return { }; }
      std::suspend_never initial_suspend() { return { }; }
      std::suspend_never final_suspend() noexcept { return { }; }
      void return_void() { }
      void unhandled_exception() { }
    };
  };

  tmpl_task<int> f(int, int)
  {
    co_await awaitable{ };
  }
}

namespace use_in_template_context
{
  template<typename T>
  struct C1
  {
    constexpr operator bool() const
    {
      T();                      // expected in non-gcc/msvc mode: expression has no effect
      return true;
    }
  };

  template<typename T>
  struct C2
  {
    constexpr operator bool() const
    {
      T();                      // expected in non-gcc/msvc mode: expression has no effect
      return true;
    }
  };

  template<typename T>
  struct tmpl_task
  {
    struct promise_type
    {
      promise_type() noexcept(C1<T>()) { T::dont_instantiate; }
      promise_type(int, int) noexcept(C2<T>()) { T::dont_instantiate; }
      ~promise_type() { T::dont_instantiate; }

      tmpl_task get_return_object() { return { }; }
      std::suspend_never initial_suspend() { return { }; }
      std::suspend_never final_suspend() noexcept { return { }; }
      void return_void() { }
      void unhandled_exception() { }
    };
  };

  template<typename T>
  tmpl_task<int> f()
  {
    co_await awaitable{ };
  }

  template<typename T>
  tmpl_task<int> f(int, int)
  {
    co_await awaitable{ };
  }
}
