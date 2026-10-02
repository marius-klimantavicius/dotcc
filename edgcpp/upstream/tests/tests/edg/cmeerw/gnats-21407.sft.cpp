//type:fp
//options:--c++11:--c++20:--ms_c++17

namespace minimal
{
  struct C {
    static int g();
  };
  template<typename T, typename ... U>
  auto f(T t, U ... u) -> decltype(C::g(u ...));
  int i = f<int>(1);
}

namespace use_param_id
{
  struct C {
    static int g(int, int);
  };

  template<typename T, typename ... U>
  auto f(T t, U ... u) -> decltype(C::g(u ...));

  int i1 = f<int>(1, 2, 3);
  int i2 = f<int, int>(1, 2, 3);
  int i3 = f<int, int, int>(1, 2, 3);
}

namespace use_param_type
{
  struct C {
    static int g(int, int);
  };

  template<typename T, typename ... U>
  auto f(T t, U ... u) -> decltype(C::g(U() ...));

  int i1 = f<int>(1, 2, 3);
  int i2 = f<int, int>(1, 2, 3);
  int i3 = f<int, int, int>(1, 2, 3);
}

namespace use_param_id_and_param_type
{
  struct C {
    static int g(int, int);
  };

  template<typename T, typename ... U>
  auto f(T t, U ... u) -> decltype(C::g(u + U() ...));

  int i1 = f<int>(1, 2, 3);
  int i2 = f<int, int>(1, 2, 3);
  int i3 = f<int, int, int>(1, 2, 3);
}
