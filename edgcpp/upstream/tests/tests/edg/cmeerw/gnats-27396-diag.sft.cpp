//type:fn
//options:--c++20:--ms_c++20 --microsoft_version 1936
//options_all:-tused -w

template<typename, int>
concept X = true;

template<typename T, int>
using Y = T;

template<typename T>
int f() {
  X<T::val> auto i = 1;
  X<int, T::val>;

  X<Y<T, T::val>{}> auto j = 1;
  X<Y<T, T::val>, 0>;

  Y<T, T::val> k = 1;
  Y<T, T::val>{};

  return i + j + k;
}

int i = f<int>();
