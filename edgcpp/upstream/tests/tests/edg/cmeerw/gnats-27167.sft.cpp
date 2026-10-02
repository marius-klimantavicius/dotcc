//type:fp
//options:--c++11:--c++11 --gn 130200:--c++11 --clang_version 180200:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<typename T> using fn_t = T(T);
  template<typename T> fn_t<T> f;
  template<>
  int f(int) {
    return 0;
  }
  auto v = f(1);
}

namespace non_tmpl_decl
{
  using fn_t = int();
  fn_t f;

  auto v = f();
}

namespace tmpl_decl
{
  template<typename T>
  using fn_t = T();

  fn_t<int> f;

  auto v = f();
}

namespace tmpl_spec
{
  template<typename T>
  using fn_t = T();

  template<typename T>
  fn_t<T> f;

  template<>
  fn_t<int> f;

  auto v = f<int>();
}

namespace tmpl_member
{
  template<typename T>
  using fn_t = T();

  template<typename T>
  struct C {
    fn_t<int> f;
  };

  auto v = C<int>().f();
}
