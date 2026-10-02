//type:fp
//options:--c++11 --no_exceptions:--c++17 --no_exceptions:--c++11:--c++17

namespace minimal
{
  template<typename T>
  struct C {
    template<typename>
    void f(int) noexcept(T::v);
  };
  C<int> c;
}

namespace non_template_member
{
  template<typename T>
  struct C {
    void f(int) noexcept(T::v);
  };
  C<int> c;
}
