//type:fp
//options:--c++11:--c++20:--c++20 --gn 140200:--c++20 --clang_version 200100:--ms_c++20 --microsoft_version 1942

namespace minimal
{
  template<typename> struct X {
    using type = int;
  };
  template<typename T, typename X<T>::type * = nullptr>
  struct C { };
  template<typename T> int f(C<T>);
  int i = f(C<int>());  // Previously a spuriour error.  Now okay.
}

namespace non_default_arg
{
  template<typename>
  struct X
  {
    using type = int;
  };

  template<typename T, typename X<T>::type *>
  struct C
  { };

  template<typename T>
  int f(C<T, nullptr>);

  int i = f(C<int, nullptr>());
}

namespace integral_type
{
  template<typename> struct X
  {
    using type = int;
  };

  template<typename T, typename X<T>::type = 0>
  struct C
  { };

  template<typename T>
  int f(C<T>);

  int i = f(C<int>());
}

namespace nullptr_pointer_type
{
  template<typename> struct X
  {
    using type = int;
  };

  template<typename T, typename X<T>::type * = nullptr>
  struct C
  { };

  template<typename T>
  int f(C<T>);

  int i = f(C<int>());
}

namespace nullptr_dpdt_pointer_type
{
  template<typename T> struct X
  {
    using type = T;
  };

  template<typename T, typename X<T>::type * = nullptr>
  struct C
  { };

  template<typename T>
  int f(C<T>);

  int i = f(C<int>());
}

namespace non_null_pointer_type
{
  template<typename> struct X
  {
    using type = int;
  };

  int g;

  template<typename T, typename X<T>::type * = &g>
  struct C
  { };

  template<typename T>
  int f(C<T>);

  int i = f(C<int>());
}

namespace nullptr_member_pointer_type
{
  struct B
  { };

  template<typename> struct X
  {
    using type = int;
  };

  template<typename T, typename X<T>::type B::* = nullptr>
  struct C
  { };

  template<typename T>
  int f(C<T>);

  int i = f(C<int>());
}

namespace nullptr_dpdt_cls_member_pointer_type
{
  struct B
  { };

  template<typename T, int T::* = nullptr>
  struct C
  { };

  template<typename T>
  int f(C<T>);

  int i = f(C<B>());
}

#if __cpp_nontype_template_args
namespace class_type
{
  struct B
  {
    constexpr B(int) { }
  };

  template<typename> struct X
  {
    using type = B;
  };

  template<typename T, typename X<T>::type = 0>
  struct C
  { };

  template<typename T>
  int f(C<T>);

  int i = f(C<int>());
}
#endif
