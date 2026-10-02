//type:fn
//options:--c++14:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1942
//options_all:-w -tused

namespace minimal
{
  template<int I, int J> struct C { };
  template<typename T>   bool u;
  template<int I>        bool u<int[I]>;
  template<int I, int J> bool u<int[I][J]>;
  constexpr bool v1 = u<int[1]>;
  constexpr bool v2 = u<int[1][2]>;
}

namespace file_scope
{
  template<typename T, int S>
  struct C { };

  template<typename T>
  bool u;

  template<typename U, int S>
  bool u<C<U, S>>;

  template<>
  bool u<C<int, 2>>;

  constexpr bool v0 = u<int>;
  constexpr bool v1 = u<C<int, 1>>;
  constexpr bool v2 = u<C<int, 2>>;
}

namespace static_data_member
{
  template<typename T, int S>
  struct C { };

  template<typename V>
  struct D
  {
    template<typename T>
    static bool u;

    template<typename U, int S>
    static bool u<C<U, S>>;
  };

  template<typename V>
  struct D<V *>
  {
    template<typename T>
    static bool u;

    template<typename U, int S>
    static bool u<C<U, S>>;
  };

  template<>
  struct D<int>
  {
    template<typename T>
    static bool u;

    template<typename U, int S>
    static bool u<C<U, S>>;
  };

  constexpr bool vv0 = D<void>::u<int>;
  constexpr bool vv1 = D<void>::u<C<int, 1>>;

  constexpr bool vi0 = D<int>::u<int>;
  constexpr bool vi1 = D<int>::u<C<int, 1>>;

  constexpr bool vp0 = D<int *>::u<int>;
  constexpr bool vp1 = D<int *>::u<C<int, 1>>;
}

namespace var {
  template<typename> int v = 1;
  template<int I>    int v<int[I]> = 1;
  template<>         int v<void> = 1;

  template int v<int[1]>;
  template int v<int[1]>;

  constexpr int i1 = v<int>;
  constexpr int i2 = v<int[2]>;
  constexpr int i3 = v<void>;
}

namespace member {
  template<typename U>
  struct C {
    template<typename> static inline int v = 1;
    template<int I>    static inline int v<int[I]> = 1;
    template<>                inline int v<void> = 1;

    template<typename> struct D { };
    template<int I>    struct D<int[I]> { };
    template<>         struct D<void> { };
  };

  constexpr int i1 = C<char>::v<int>;
  constexpr int i2 = C<char>::v<int[2]>;
  constexpr int i3 = C<char>::v<void>;

  constexpr int j1 = C<char>::D<int>::x;
  constexpr int j2 = C<char>::D<int[3]>::x;
  constexpr int j3 = C<char>::D<void>::x;
}

namespace cls {
  template<typename> struct C { };
  template<int I>    struct C<int[I]> { };

  template struct C<int>;
  template struct C<int>;

  template struct C<int[1]>;
  template struct C<int[1]>;
}

namespace instantiation_context
{
  int non_const;

  template<typename S>
  struct O
  {
    template<typename T>
    struct C
    {
      static constexpr int v = non_const;

      template<typename U>
      static constexpr int vt = non_const;

      template<typename V>
      static constexpr int vt<V *> = non_const;
    };

    template<typename T>
    struct C<T *>
    {
      static constexpr int v = non_const;

      template<typename U>
      static constexpr int vt = non_const;

      template<typename V>
      static constexpr int vt<V *> = non_const;
    };
  };

  constexpr int v = O<int>::C<int>::v +
                    O<int>::C<int>::vt<int> +
                    O<int>::C<int>::vt<int *> +
                    O<int>::C<int *>::v +
                    O<int>::C<int *>::vt<int> +
                    O<int>::C<int *>::vt<int *>;
}
