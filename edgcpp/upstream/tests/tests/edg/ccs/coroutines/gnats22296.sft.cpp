//type:fn
//options:--c++20:--microsoft_version 1920
//options_all:--set_flag coroutines -tused

#include <coroutine>
using namespace std;

struct A {};

task<int> foo() {
  return {};
}

task<int> bar() {
  co_return {};
}

task<int> baz() {
  co_yield 1;
  if (0) {
    return {}
  } else {
    co_return 0;
  }
}
