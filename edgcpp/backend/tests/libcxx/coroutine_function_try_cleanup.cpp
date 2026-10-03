#include <coroutine>
#include <cstdlib>
static int result;
struct Payload { int value; Payload(int input):value(input){} ~Payload(){result += 10;} };
static void raise_payload() { throw Payload(3); }
struct Task {
  struct promise_type {
    static void* operator new(decltype(sizeof(0)) size) { return std::malloc(size); }
    static void operator delete(void* address) noexcept { std::free(address); }
    Task get_return_object() noexcept { return {}; }
    std::suspend_never initial_suspend() noexcept { return {}; }
    std::suspend_never final_suspend() noexcept { return {}; }
    void return_void() noexcept {}
    void unhandled_exception() noexcept { result += 100; }
  };
};
static Task work() try { raise_payload(); co_return; }
catch (Payload value) { result += value.value; throw; }
int main() { work(); return result == 123 ? 0 : 1; }
