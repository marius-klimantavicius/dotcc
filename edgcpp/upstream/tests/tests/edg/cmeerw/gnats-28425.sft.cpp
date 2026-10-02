//type:fp
//options:--c++11:--c++11 --gn 150100:--c++11 --clang_version 210100

namespace minimal
{
  template<typename T> struct D {
    template<typename U>
    friend void f(D, U) noexcept(T::v);
  };
  template<typename> struct C;
  template<> struct C<int> {
    D<int> d;
  };
}

namespace normal_inst
{
  template<typename T>
  struct D
  {
    template<typename U>
    friend void f(D, U) noexcept(T::v);
  };

  D<int> d;
}

namespace explicit_inst
{
  template<typename T>
  struct D
  {
    template<typename U>
    friend void f(D, U) noexcept(T::v);
  };

  template struct D<int>;
  D<int> d;
}

namespace inst_template
{
  template<typename T>
  struct D
  {
    template<typename U>
    friend void f(D, U) noexcept(T::v);
  };

  template<typename T>
  struct C {
    D<int> d;
  };

  C<int> c;
}

namespace inst_explicit_spec
{
  template<typename T>
  struct D
  {
    template<typename U>
    friend void f(D, U) noexcept(T::v);
  };

  template<typename T>
  struct C
  { };

  template<>
  struct C<int>
  {
    D<int> d;
  };
}

namespace non_template_friend
{
  template<typename T>
  struct D
  {
    void f(D) noexcept(T::v);
  };

  template<typename T>
  struct C
  { };

  template<>
  struct C<int>
  {
    D<int> d;
  };
}
