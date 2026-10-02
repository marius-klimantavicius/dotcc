//type:fn
//options_all:-tused -w
//options:--c++11:--c++11 --g++:--c++11 --microsoft

namespace cast_expr_stmt
{
  template<typename T, typename ... UU>
  void f(T t, UU ... uu)
  {
    (T(uu ..., t));             // error
    (T(t, uu ...));             // error

    (T(uu ..., uu ..., t));     // error
    (T(t, uu ..., uu ...));     // error

    (T(uu ..., t, uu ...));     // error
  }

  template void f(int, int);
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
    f_id(1, 2);
    f_id_pack(1, 2);            // error
    f_pack_id(1, 2);            // error
    f_id_pack_pack(1, 2);       // error
    f_pack_pack_id(1, 2);       // error
    f_pack_id_pack(1, 2);       // error

    f(1, 2);
    f_pack(1, 2);
    f_pack_pack(1, 2);          // error
  }
}
