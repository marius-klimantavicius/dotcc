//type:fp
//options:--c++14:--c++20:--c++20 --g++ --gnu_version=120100:--ms_c++20
//options_all:-w

namespace minimal
{
  template<typename T>
  struct C {
    template<typename U>
    static T s;
  };
  template<typename T> template<typename U>
  T C<T>::s<U *> = 1;
  int i = C<int>::s<int *>;
}

namespace static_data_member
{
  template<typename T>
  struct C
  {
    template<typename U>
    static T s;

    static constexpr int i = 1;
  };

  constexpr int i = 0;

  template<typename T>
  template<typename U>
  constexpr T C<T>::s<U *> = i;

  static_assert(C<int>::s<int *> == 1, "use partial specialization");
}

namespace nested_class_template
{
  using type = void;
  constexpr int i = 0;

  template<typename T>
  struct C
  {
    template<typename U>
    struct D;

    using type = int;

    static constexpr int i = 1;
  };

  template<typename T>
  template<typename U>
  struct C<T>::D
  {
    type t;

    static constexpr int v = i;
  };

  template<typename T>
  template<typename U>
  struct C<T>::D<U *>
  {
    type t;

    static constexpr int v = i + 1;
  };

  static_assert(C<int>::D<int>::v == 1, "primary template");
  static_assert(C<int>::D<int *>::v == 2, "partial specialization");
}
