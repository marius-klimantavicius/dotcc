// The source handler must run before promise.unhandled_exception.
#include <coroutine>
#include <cstdlib>
static int result;
struct Task {
  struct promise_type {
    static void* operator new(decltype(sizeof(0)) size) { return std::malloc(size); }
    static void operator delete(void* address) noexcept { std::free(address); }
    Task get_return_object() noexcept { return {}; }
    std::suspend_never initial_suspend() noexcept { return {}; }
    std::suspend_never final_suspend() noexcept { return {}; }
    void return_void() noexcept { result += 2; }
    void unhandled_exception() noexcept { result += 100; }
  };
};
static Task work() try { throw 3; co_return; }
catch (int value) { result += value; throw; }
int main() { work(); return result == 103 ? 0 : 1; }
