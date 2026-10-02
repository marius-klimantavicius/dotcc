//type:fn
//options:--c++23

namespace incomplete_base_class
{
  template<typename T>
  struct B;

  template<typename T>
  struct D : B<T>
  {
    using B<T>::B;
  };

  D d(1);                       // error: cannot deduce
}

namespace non_deducible
{
  template<typename T>
  struct B
  { };

  template<typename T>
  struct D : B<typename T::type>
  {
    using B<typename T::type>::B;
  };

  D d(1);                       // error: cannot deduce
}

namespace possible_assertion
{
  struct A;

  template<auto V = A()>
  struct S { };

  S s;                          // error: cannot deduce
}

namespace explicit_deduction_guide
{
  template<typename T>
  struct B
  {
    B(int);
  };

  template<typename T>
  explicit B(T) -> B<T>;

  template<typename T>
  struct D : B<T>
  {
    using B<T>::B;
  };

  D d = 1;                      // error: cannot deduce - explicit deduction guide
}

namespace explicit_constructor
{
  template<typename T>
  struct B
  {
    explicit B(T);
  };

  template<typename T>
  struct D : B<T>
  {
    using B<T>::B;
  };

  D d = 1;                      // error: cannot deduce - explicit constructor/guide
}
