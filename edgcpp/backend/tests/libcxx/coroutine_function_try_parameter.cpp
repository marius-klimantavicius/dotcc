#include <coroutine>
#include <cstdio>
#include <cstdlib>
static int mode;
static void note(int value) {
  if (value < 0) { std::putchar('-'); value = -value; }
  if (value >= 10) std::putchar('0' + value / 10);
  std::putchar('0' + value % 10); std::putchar(',');
}
struct Payload {
  int value;
  Payload(int input) : value(input) { note(10 + value); }
  Payload(const Payload& source) : value(source.value + 1) { note(10 + value); }
  ~Payload() { note(-value); }
};
static void raise_payload() { throw Payload(1); }
struct Task {
  struct promise_type;
  using Handle = std::coroutine_handle<promise_type>;
  Handle handle;
  struct promise_type {
    static void* operator new(decltype(sizeof(0)) size) { note(1); return std::malloc(size); }
    static void operator delete(void* address) noexcept { note(9); std::free(address); }
    Task get_return_object() noexcept { return {Handle::from_promise(*this)}; }
    std::suspend_always initial_suspend() noexcept { return {}; }
    std::suspend_always final_suspend() noexcept { return {}; }
    void return_void() noexcept { note(8); }
    void unhandled_exception() {
      note(5);
      try { throw; }
      catch (const Payload& object) { note(20 + object.value); }
      catch (int value) { note(value); }
    }
  };
};
static Task work() try { raise_payload(); co_return; }
catch (Payload object) {
  note(3); note(30 + object.value);
  if (mode == 1) throw 42;
  throw;
}
int main(int argc, char** argv) {
  mode = argc > 1 ? argv[1][0] - '0' : 0;
  Task task = work(); note(2);
  task.handle.resume(); note(6);
  if (!task.handle.done()) return 1;
  task.handle.destroy(); std::puts("");
}
