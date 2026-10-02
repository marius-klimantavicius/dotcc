//type:fp
//options_all:--microsoft_version 1900 --set_flag coroutines -tused

#include <coroutine>
std::generator<int> f() {
  for (int i = 0; i < 1; i++)
    co_yield 0;
  co_return 0;
}
