//type:fp
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

struct Future {
  bool await_ready();
  void await_suspend(coroutine_handle<>);
  int await_resume();
};

Future f();

task<int> g() {
  for (int i = co_await f(); i < 10; ++i) {}
  co_return 1;
}
