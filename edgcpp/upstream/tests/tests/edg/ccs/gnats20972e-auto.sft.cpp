//type:fn
//options:--clang_version 30200
//options_all:--c++11 --sys_include $RUN_TEST_CURR_DIR/include

#include <initializer_list>

template<typename T, typename U> struct same;
template<typename T> struct same<T, T> { };

void f() {
  auto a = new auto {2};
}
