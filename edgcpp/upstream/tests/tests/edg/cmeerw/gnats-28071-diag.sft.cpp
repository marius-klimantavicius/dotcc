//type:fn
//options:--c++14:--c++14 --gn 140200

namespace name_qualifier_class
{
  class Y { };
  template<typename> using D = Y;
  template<typename T> int v = D<T>::v; // error (except GCC)
}

namespace name_qualifier_struct
{
  struct Y { };
  template<typename> using D = Y;
  template<typename T> int v = D<T>::v; // error (except GCC)
}

namespace name_qualifier_union
{
  union Y { };
  template<typename> using D = Y;
  template<typename T> int v = D<T>::v; // error (except GCC)
}

namespace in_tmpl_arg_list_struct
{
  struct Y { };
  template<typename> using A = int;
  template<typename> using D = Y;
  template<typename T> int v = D<A<T>>::v; // error (except GCC)
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
    using B::f;                 // error (except GCC)

    void g()
    {
      this->f();                // error (except GCC)
    }
  };
}

namespace name_qualifier_pointer
{
  struct Y { };
  template<typename> using D = Y *;
  template<typename T> int v = D<T>::v; // error
}

namespace in_tmpl_arg_list_pointer
{
  struct Y { };
  template<typename> using A = int;
  template<typename> using D = Y *;
  template<typename T> int v = D<A<T>>::v; // error
}
