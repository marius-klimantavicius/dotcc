// Selected allocator declarations must have a source provider before emission.
#include <coroutine>
#include <cstdlib>
struct Task {
  struct promise_type {
    static void* operator new(decltype(sizeof(0)) size);
    static void operator delete(void* address) noexcept;
    Task get_return_object() noexcept { return {}; }
    std::suspend_never initial_suspend() noexcept { return {}; }
    std::suspend_never final_suspend() noexcept { return {}; }
    void return_void() noexcept {}
    void unhandled_exception() noexcept { std::abort(); }
  };
};
#ifndef OMIT_ALLOCATOR
void* Task::promise_type::operator new(decltype(sizeof(0)) size) { return std::malloc(size); }
#endif
#ifndef OMIT_DEALLOCATOR
void Task::promise_type::operator delete(void* address) noexcept { std::free(address); }
#endif
static Task work() { co_return; }
int main() { work(); return 0; }
