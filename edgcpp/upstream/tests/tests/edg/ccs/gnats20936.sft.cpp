//type:fp
//options_all:--c++17 -w --sys_include $RUN_TEST_CURR_DIR/include

#include <initializer_list>

template<typename T> struct X {
  X(std::initializer_list<T>) = delete;
  X(const X&);
};

template<typename T> struct Y : X<float> {};

void bar(X<int>& x, Y<int>& y) {
  X x1{x};
  X x2(x);
  X x3{y};
  X x4(y);

  X{x};
  X(x);
  X{y};
  X(y);
}
