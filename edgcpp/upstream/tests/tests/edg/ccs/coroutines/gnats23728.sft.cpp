//type:cp
//options:--c++20:--microsoft_version 1928 --ms_c++latest
//options_all:--set_flag coroutines -tused

#include <coroutine>

auto g = []() -> std::generator<int> {
  co_yield 1;
  co_return 2;
};
