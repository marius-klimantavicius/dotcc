//type:fn
//options::--clang_version 30300:--gnu_version 40900
//options_all:--c++14 --sys_include $RUN_TEST_CURR_DIR/include

#include <initializer_list>

template<typename T, typename U> struct same;
template<typename T> struct same<T, T> { };

void f() {
  decltype(auto) a({2});
}
