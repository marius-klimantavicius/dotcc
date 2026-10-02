//type:fp
//options:--c++14 --gn 140200

namespace name_qualifier_class
{
  class Y { };
  template<typename> using D = Y;
  template<typename T> int v = D<T>::v;
}

namespace name_qualifier_struct
{
  struct Y { };
  template<typename> using D = Y;
  template<typename T> int v = D<T>::v;
}

namespace name_qualifier_union
{
  union Y { };
  template<typename> using D = Y;
  template<typename T> int v = D<T>::v;
}

namespace in_tmpl_arg_list_struct
{
  struct Y { };
  template<typename> using A = int;
  template<typename> using D = Y;
  template<typename T> int v = D<A<T>>::v;
}

namespace dpdt_base_class_struct
{
  struct Y { };
  template<typename> using A = Y;

  template<typename T>
  struct D : A<T>
  {
    using B = A<T>;

    using B::B;
    using B::f;
  };
}

namespace incomplete_base_class
{
  struct B;

  template<typename T>
  using A = B;

  template<typename T>
  struct D : A<T>
  {
    using A<T>::A;
    using A<T>::f;
  };

  struct B
  {
    void f();
  };

  D<void> d;
}
