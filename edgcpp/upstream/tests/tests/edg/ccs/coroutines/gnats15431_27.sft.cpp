//type:fp
//options:--microsoft_version 1900
//options_all:--set_flag coroutines -tused

#include <coroutine>
struct suspend_never
{
  bool await_ready() noexcept { return true; }
  void await_suspend(std::coroutine_handle<>) noexcept {}
  void await_resume() noexcept {}
};
std::task<int> f()
{
  co_await suspend_never{};
  co_return { };
}
