//type:fp
//options:--microsoft_version 1900
//options_all:--set_flag coroutines -tused

#include <coroutine>
std::generator<int> f()
{
  co_yield 1;
  co_return 1;
}
