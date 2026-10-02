//type:fp
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

struct A {
  A* begin();
  A* end();
};

struct Future {
  bool await_ready();
  void await_suspend(coroutine_handle<>);
  A await_resume();
};

Future f();

task<int> g() {
  for (auto i: co_await f()) {}
  co_return 1;
}
