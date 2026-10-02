//options_all:--set_flag coroutines --no_ms_perm --microsoft_v 1923
namespace std {
    template <typename Coroutine>
    struct coroutine_traits {
        using promise_type = typename Coroutine::promise_type;
    };
 
    template <typename Promise = void>
    struct coroutine_handle;
 
    template <>
    struct coroutine_handle<void> {};
 
    template <typename Promise>
    struct coroutine_handle : coroutine_handle<> {};
 
    struct suspend_never {
        bool await_ready() noexcept { return true; }
        void await_suspend(coroutine_handle<>) noexcept {}
        void await_resume() noexcept {}
    };
}
 
struct coro {
    struct promise_type {
        std::suspend_never initial_suspend() { return {}; }
        std::suspend_never final_suspend() noexcept { return {}; }
        void return_void() {}
       coro get_return_object() { return {}; }
        void unhandled_exception() {}
    };
};
 
struct Immovable {
    Immovable() = default;
    Immovable(Immovable const &) = delete;
    Immovable(Immovable &&) = delete;
    Immovable &operator=(Immovable const &) = delete;
    Immovable &operator=(Immovable &&) = delete;
};
 
Immovable imm1;
 
struct Aw {
    bool await_ready() { return true; }
    void await_suspend(std::coroutine_handle<>) {}
    Immovable &await_resume() { return imm1; }
};
 
coro f() {
    Immovable &r1 = co_await Aw{};
}
