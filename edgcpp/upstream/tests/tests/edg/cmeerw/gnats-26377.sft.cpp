//type:fp
//options:--c++20:--ms_c++20

namespace minimal
{
  template<typename T> struct X;
  template<typename T> concept C0 = true;
  template<typename T> concept C1 = C0<X<T>>;
  template<typename T> concept C2 = C0<X<T>> && true;
  template<typename> struct P;
  template<C1 C> struct P<C> { };
  template<C2 C> struct P<C> { };
  P<int> p;
}

namespace requires_clause
{
  template<typename T> struct X;

  template<typename T> concept C0 = true;
  template<typename T> concept C1 = C0<X<T>>;
  template<typename T> concept C2 = C0<X<T>> && true;

  template<typename> struct P;

  template<typename T> requires C1<T> struct P<T> { };
  template<typename T> requires C2<T> struct P<T> { };

  P<int> p;
}

namespace no_nesting
{
  template<typename T> struct X;

  template<typename T> concept C0 = true;
  template<typename T> concept C1 = C0<T>;
  template<typename T> concept C2 = C0<T> && true;

  template<typename> struct P;
  template<C1 C> struct P<C> { };
  template<C2 C> struct P<C> { };

  P<int> p;
}

namespace in_fn_return_type
{
  template<typename T> struct X;

  template<typename T> concept C0 = true;
  template<typename T> concept C1 = C0<T()>;
  template<typename T> concept C2 = C0<T()> && true;

  template<typename> struct P;
  template<C1 C> struct P<C> { };
  template<C2 C> struct P<C> { };

  P<int> p;
}

namespace in_fn_param_type
{
  template<typename T> struct X;

  template<typename T> concept C0 = true;
  template<typename T> concept C1 = C0<void(T)>;
  template<typename T> concept C2 = C0<void(T)> && true;

  template<typename> struct P;
  template<C1 C> struct P<C> { };
  template<C2 C> struct P<C> { };

  P<int> p;
}

namespace in_array_type
{
  template<typename T> struct X;

  template<typename T> concept C0 = true;
  template<typename T> concept C1 = C0<T[1]>;
  template<typename T> concept C2 = C0<T[1]> && true;

  template<typename> struct P;
  template<C1 C> struct P<C> { };
  template<C2 C> struct P<C> { };

  P<int> p;
}

namespace in_mbr_ptr_to_type
{
  struct D
  { };

  template<typename T> struct X;

  template<typename T> concept C0 = true;
  template<typename T> concept C1 = C0<T D::*>;
  template<typename T> concept C2 = C0<T D::*> && true;

  template<typename> struct P;
  template<C1 C> struct P<C> { };
  template<C2 C> struct P<C> { };

  P<int> p;
}

namespace in_mbr_ptr_class_type
{
  struct D
  { };

  template<typename T> struct X;

  template<typename T> concept C0 = true;
  template<typename T> concept C1 = C0<int T::*>;
  template<typename T> concept C2 = C0<int T::*> && true;

  template<typename> struct P;
  template<C1 C> struct P<C> { };
  template<C2 C> struct P<C> { };

  P<D> p;
}
