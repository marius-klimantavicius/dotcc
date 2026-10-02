//type:fp
//options:--c++:--c++17:--c++20:--c++ --clang_version 40100:--c++20 --clang_version 160000:--c++ --gn 120100:--c++20 --gn 120100

namespace minimal
{
  template<int I, int J> struct A;
  template<int I>
  struct A<I, I*2> { };
}

namespace dpdt_arg
{
  template<typename, int>
  struct C;

  template<typename T>
  struct C<T, sizeof(T)>
  { };

  C<int, sizeof(int)> c;
}

namespace dpdt_arg_dpdt
{
  template<typename, int>
  struct C;

  template<typename T>
  struct C<T, T(0)>             // error on clang
  { };

  C<int, 0> c;               // clang is unable to match partial specialization
}

namespace non_specialized_arg_dependent_type
{
  template<typename T, T t, typename U>
  struct C;

  template<typename T, T t, typename U>
  struct C<T, t, U *>
  { };

  C<int, 1, int *> c;
}

namespace specialized_arg_instantiated_type
{
  template<class T, class U, U u>
  struct C;

  template<class T>
  struct C<T, int, 1>
  { };

  C<int, int, 1> c;
}

#if __cpp_decltype
namespace specialized_arg_instantiated_decltype_type
{
  template<class T, class U, decltype(U()) u>
  struct C;

  template<class T>
  struct C<T, int, 1>
  { };

  C<int, int, 1> c;
}
#endif

#if __cpp_nontype_template_parameter_auto
namespace specialized_arg_for_deduced_type
{
  template<typename T, auto t>
  struct C;

  template<typename T>
  struct C<T, 1>
  { };

  C<int, 1> c;
}

namespace specialized_arg_for_nondpdt_deduced_type
{
  template<typename T, auto t>
  struct C;

  template<typename T>
  struct C<T, sizeof(T)>        // error on clang
  { };

  C<int, sizeof(int)> c;        // error on clang
}
#endif

#if __cpp_nontype_template_args >= 201911L
namespace specialized_arg_class_type_nttp
{
  template<typename T, typename U, T>
  struct C;

  template<typename T>
  struct C<bool, T, T::value>
  { };

  struct X
  {
    int m;

    constexpr X(int i)
      : m(i)
    { }

    static constexpr int value{};
  };

  C<bool, X, 0> c;
}
#endif

namespace nested_class_outer_type
{
  template<typename U>
  struct B
  {
    template<typename T, U I>
    struct C;

    template<typename T>
    struct C<T, T(0)>           // error on clang
    { };
  };

  B<int>::C<int, 0> c;       // clang is unable to match partial specialization
}
