// Trivial promise/parameters: exercise the source function-try boundary itself.
#include <coroutine>
#include <cstdio>
#include <cstdlib>
static int mode;
static void note(int value) {
  if (value < 0) { std::putchar('-'); value = -value; }
  if (value >= 100) std::putchar('0' + value / 100);
  if (value >= 10) std::putchar('0' + value / 10 % 10);
  std::putchar('0' + value % 10); std::putchar(',');
}
struct Initial {
  bool await_ready() { note(10); if (mode == 2) throw 72; return false; }
  bool await_suspend(std::coroutine_handle<>) { note(11); return mode == 3 || mode == 9 || mode == 10; }
  void await_resume() { note(12); if (mode == 4) throw 74; }
};
struct BodyAwait {
  bool await_ready() noexcept { note(21); return false; }
  bool await_suspend(std::coroutine_handle<>) noexcept { note(22); return mode == 7; }
  void await_resume() { note(23); if (mode == 8) throw 78; }
};
struct Final {
  bool await_ready() noexcept { note(40); return false; }
  void await_suspend(std::coroutine_handle<>) noexcept { note(41); }
  void await_resume() noexcept { note(42); }
};
struct Task {
  struct promise_type;
  using Handle = std::coroutine_handle<promise_type>;
  Handle handle;
  struct promise_type {
    static void* operator new(decltype(sizeof(0)) size) { note(1); return std::malloc(size); }
    static void operator delete(void* address) noexcept { note(-1); std::free(address); }
    Task get_return_object() { note(2); if (mode == 1) throw 71; return {Handle::from_promise(*this)}; }
    Initial initial_suspend() noexcept { return {}; }
    Final final_suspend() noexcept { return {}; }
    void return_void() noexcept { note(30); }
    void unhandled_exception() {
      note(90);
      if (mode == 10) throw 110;
      try { throw; } catch (int value) { note(value); } catch (double) { note(66); }
    }
  };
};
static Task work() try {
  note(20);
  co_await BodyAwait{};
  if (mode == 5 || mode == 9 || mode == 10) throw 75;
  if (mode == 6) throw 2.5;
  co_return;
} catch (int value) {
  note(80); note(value);
  throw;
}
int main(int argc, char** argv) {
  mode = argc > 1 ? argv[1][0] - '0' : 0;
  if (argc > 1 && argv[1][1]) mode = mode * 10 + argv[1][1] - '0';
  Task task{};
  try {
    task = work(); note(50);
    while (mode != 9 && !task.handle.done()) { task.handle.resume(); note(51); }
  } catch (int value) { note(100 + value); }
  if (task.handle) task.handle.destroy();
  std::puts("");
}
