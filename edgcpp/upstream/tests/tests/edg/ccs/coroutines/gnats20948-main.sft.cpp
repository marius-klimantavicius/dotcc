//type:fn
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

int main() {
  co_await 0;
}
