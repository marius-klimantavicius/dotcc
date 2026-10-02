//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1936
//options_all:-tused -w

namespace minimal
{
  template<typename ...> concept X = true;
  template<typename> struct C {};
  template<typename ... Us>
  void f() {
    X<C<Us> ...> auto i = 1;
  }
  template void f<>();
}


template<typename T, int, typename ... Vs>
concept X = true;

template<typename T, int, typename ... Vs>
using Y = T;

template<typename T, int, typename ... Vs>
T var = 0;

template<typename W>
struct C { };

template<typename T, typename ... Us>
int f()
{
  X<1, C<Us> ...> auto i = 1;
  X<int, 1, C<Us> ...>;

  Y<T, 1, C<Us> ...> j = { };
  Y<T, 1, C<Us> ...>{};

  var<T, 1, C<Us> ...>;

  return i + j;
}

int i = f<int>();
