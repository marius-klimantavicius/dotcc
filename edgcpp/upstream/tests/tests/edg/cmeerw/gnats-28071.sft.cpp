//type:fn
//options:--c++11:--c++11 --gn 140200:--c++11 --gn 140200:--c++11 --clang_version 200100:--c++11 --clang_version 200100:--ms_c++20 --microsoft_version 1942

struct S {};

namespace minimal
{
  struct B;
  template<typename T> using A = B;
  template<typename T>
  struct D : A<T> {             // accepted by gcc
    using A<T>::A;              // accepted by gcc
  };
  struct B { };
  D<void> d;
}

namespace non_dpdt_class
{
  template<typename> using A = S;
  template<typename> using I = int;

  template<typename T> struct C : A<T>::X { };     // accepted by gcc
  template<typename T> struct CI : A<I<T>>::X { }; // accepted by gcc

  template<typename T> struct D : A<T> {
    using B = A<T>;

    using B::B;                 // accepted by gcc
    using B::f;                 // accepted by gcc
  };

  template<typename T> struct DI : A<I<T>> {
    using B = A<I<T>>;

    using B::B;                 // accepted by gcc
    using B::f;                 // accepted by gcc
  };
}

namespace scalar
{
  template<typename T> using A = int;
  template<typename T> struct C : A<T>::X { }; // error

  template<typename T> struct D : A<T> // error
  { };
}

namespace ptr_to_non_dpdt_class
{
  template<typename T> using A = S *;
  template<typename T> struct C : A<T>::X { }; // error

  template<typename T> struct D : A<T> // error
  { };
}

namespace ptr_to_scalar
{
  template<typename T> using A = int *;
  template<typename T> struct C : A<T>::X { }; // error

  template<typename T> struct D : A<T> // error
  { };
}

namespace dpdt_type
{
  template<typename T> using A = T;
  template<typename T> struct C : A<T>::X { }; // OK

  template<typename T> struct D : A<T> {
    using B = A<T>;

    using B::B;
    using B::f;
  };
}

namespace ptr_to_dpdt_type
{
  template<typename T> using A = T *;
  template<typename T> struct C : A<T>::X { }; // error

  template<typename T> struct D : A<T> // error
  { };
}
