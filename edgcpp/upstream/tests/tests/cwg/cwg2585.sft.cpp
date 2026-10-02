//type:fn
//options_all:--c++23 -tused -A
#include<coroutine>

struct resumable {
   struct promise_type;
   using coro_handle = std::coroutine_handle<promise_type>;

   resumable(coro_handle handle) : handle_(handle) {}
   resumable(resumable&) = delete;
   ~resumable() { if (handle_) { handle_.destroy(); } }

   coro_handle handle_;
};

struct Allocator;

struct resumable::promise_type {
   void* operator new(std::size_t sz, Allocator&);

   std::coroutine_handle<promise_type> get_return_object() { return std::coroutine_handle<promise_type>::from_promise(*this); }
   auto initial_suspend() { return std::suspend_always(); }
   auto final_suspend() noexcept { return std::suspend_always(); }
   void unhandled_exception() {}
   void return_void() {};
};

resumable foo() {
    co_return;
}

//cwg: 2585
//title: Name lookup for coroutine allocation
//meeting: Virtual 7/22
//edg_status: EDGcpfe/25521
