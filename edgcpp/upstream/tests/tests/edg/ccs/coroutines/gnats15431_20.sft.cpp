//type:fp
//options:--microsoft_version 1900
//options_all:--set_flag coroutines -tused

#include <coroutine>

extern "C" int printf(char const*, ...);

struct A {
auto operator co_await() { 
  printf("member\n");
  return std::suspend_never{};
}
};
struct B : A {
bool await_ready() { printf("non-member"); return true; }
void await_suspend(std::coroutine_handle<>) {}
void await_resume() {}
};
std::task<void> f() {
co_await A{};
co_await B{}; // user probably expects that members will be called
}

