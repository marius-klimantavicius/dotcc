//type:fp
//options:--clang_version 60000
//options_all:--c++14 --sys_include $RUN_TEST_CURR_DIR/include

#include <initializer_list>

template<typename T, typename U> struct same;
template<typename T> struct same<T, T> { };

void f() {
  auto a = new decltype(auto) ({2});
  same<decltype(a), int*>();
}
