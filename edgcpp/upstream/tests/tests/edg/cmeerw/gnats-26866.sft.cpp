//type:fp
//options:--c++20:--ms_c++20

namespace minimal
{
  template<typename> struct B;
  template<typename T> requires true
  struct B<T> {
    template<int> struct N;
  };
  template<typename T> requires true
  template<int> struct B<T>::N {};
}

namespace incomplete_primary
{
  template<typename> concept C = true;

  template<typename> struct B;

  template<typename T> requires C<T>
  struct B<T> {
    template<int> struct N;
  };

  template<typename T> requires C<T>
  template<int> struct B<T>::N
  { };

  B<int>::N<0> n;
}

namespace complete_primary
{
  template<typename> concept C = true;

  template<typename> struct B
  { };

  template<typename T> requires C<T>
  struct B<T>
  {
    template<int> struct N;
  };

  template<typename T> requires C<T>
  template<int> struct B<T>::N
  { };

  B<int>::N<0> n;
}

namespace constrained_template_parameter
{
  template<typename> concept C = true;

  template<typename> struct B;

  template<C T>
  struct B<T> {
    template<int> struct N;
  };

  template<C T>
  template<int> struct B<T>::N
  { };

  B<int>::N<0> n;
}

namespace constrained_primary_template_parameter
{
  template<typename> concept C = true;

  template<C T>
  struct B {
    template<int> struct N;
  };

  template<C T>
  template<int> struct B<T>::N
  { };

  B<int>::N<0> n;
}

namespace partial_spec
{
  template<typename> concept C = true;

  template<typename> struct B;

  template<typename T> requires C<T>
  struct B<T *>
  {
    template<int> struct N;
  };

  template<typename T> requires C<T>
  template<int> struct B<T *>::N
  { };

  B<int *>::N<0> n;
}

namespace constrained_primary_with_partial_spec
{
  template<typename T> concept C = true;
  template<typename T> concept C1 = true;
  template<typename T> concept C2 = C1<T> && true;

  template<C T> requires C1<T>
  struct B { };

  template<C T> requires C2<T>
  struct B<T> {
    template<int> struct N;
  };

  template<C T> requires C2<T>
  template<int> struct B<T>::N
  { };

  B<int>::N<0> n;
}

namespace alias_template
{
  template<typename T>
  using A = int;

  template<typename T>
  A<T> a;
}

namespace variable_template
{
  template<typename T>
  constexpr int v = 0;

  template<typename T> requires true
  constexpr int v<T> = 1;

  static_assert(v<int> == 1);
}
