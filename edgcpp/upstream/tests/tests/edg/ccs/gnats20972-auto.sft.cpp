//type:fp
//options::--clang_version 30800:--gnu_version 50000
//options_all:--c++11 --sys_include $RUN_TEST_CURR_DIR/include

#include <initializer_list>

template<typename T, typename U> struct same;
template<typename T> struct same<T, T> { };

void f() {
  auto a{2};
  same<decltype(a), int>();
}
