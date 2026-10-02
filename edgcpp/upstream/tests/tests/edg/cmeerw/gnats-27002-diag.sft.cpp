//type:fn
//options:--c++20 -A:--c++20:--c++20 --gn 130200:--c++20 --clang_version 160000:--ms_c++20 --microsoft_version 1936

#include <coroutine>

struct X {
  struct promise_type {
    X get_return_object();
    auto initial_suspend() { return std::suspend_always{}; }
    auto final_suspend() noexcept { return std::suspend_always{}; }
    void unhandled_exception();
    auto yield_value(int) { return std::suspend_always{}; }
    void return_value(int);
  };
};

auto l1 = [] (int) -> X {
  co_await std::suspend_always{};
} (0);

auto l2 = []<typename T = void> (int) -> X {
  co_await std::suspend_always{};
} (0);

auto l3 = [] (auto) -> X {
  co_await std::suspend_always{};
} (1);

auto rl1 = [] (int) -> X {
  co_await std::suspend_always{};
  co_return;
} (0);

auto rl2 = []<typename T = void> (int) -> X {
  co_await std::suspend_always{};
  co_return;
} (0);

auto rl3 = [] (auto) -> X {
  co_await std::suspend_always{};
  co_return;
} (1);
