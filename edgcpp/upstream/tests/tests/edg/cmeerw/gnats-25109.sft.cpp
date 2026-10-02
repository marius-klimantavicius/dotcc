//type:fp
//options_all:-tused -w
//options:--c++11:--c++11 --g++:--c++11 --microsoft:--ms_c++20

namespace minimal
{
  template<typename T, typename ... U>
  T f(T t, U ... u) {
    return T(t, u ...);
  }
  template int f(int);
}

namespace cast_expr_stmt
{
  template<typename T, typename ... UU>
  void f(T t, UU ... uu)
  {
    T();
    T(uu ...);
    T(uu ..., uu ...);

    (T(t));

    (T(uu ..., t));
    (T(t, uu ...));

    (T(uu ..., uu ..., t));
    (T(t, uu ..., uu ...));

    (T(uu ..., t, uu ...));
  }

  template void f(int);
}

namespace in_decltype_expr
{
  template<typename T, typename ... UU>
  auto f_id(T t, UU ... uu) -> decltype(T(t));

  template<typename T, typename ... UU>
  auto f_id_pack(T t, UU ... uu) -> decltype(T(t, uu ...));

  template<typename T, typename ... UU>
  auto f_pack_id(T t, UU ... uu) -> decltype(T(uu ..., t));

  template<typename T, typename ... UU>
  auto f_id_pack_pack(T t, UU ... uu) -> decltype(T(t, uu ..., uu ...));

  template<typename T, typename ... UU>
  auto f_pack_pack_id(T t, UU ... uu) -> decltype(T(uu ..., uu ..., t));

  template<typename T, typename ... UU>
  auto f_pack_id_pack(T t, UU ... uu) -> decltype(T(uu ..., t, uu ...));

  template<typename T, typename ... UU>
  auto f(T t, UU ... uu) -> decltype(T());

  template<typename T, typename ... UU>
  auto f_pack(T t, UU ... uu) -> decltype(T(uu ...));

  template<typename T, typename ... UU>
  auto f_pack_pack(T t, UU ... uu) -> decltype(T(uu ..., uu ...));

  void g()
  {
    f_id(1);
    f_id_pack(1);
    f_pack_id(1);
    f_id_pack_pack(1);
    f_pack_pack_id(1);
    f_pack_id_pack(1);

    f(1);
    f_pack(1);
    f_pack_pack(1);
  }
}
