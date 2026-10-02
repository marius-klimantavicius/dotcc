//type:fn
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

struct Future {
  bool await_ready();
  void await_suspend(coroutine_handle<>);
  int await_resume();
};

Future g();

task<int> f() {
  int i = co_await g();
  static int j = co_await g();
  co_return j;
}
