//type:fn
//options_all:--microsoft_version 1900 --set_flag coroutines -tused

#include <coroutine>
using namespace std;
 
struct S {
  bool await_ready() { return false; }
  void await_suspend(coroutine_handle<>) {}
  void await_resume() {}
};
 
S f();
task<void> g()
{
  co_await f();
  co_return 5;  //(!)
}
