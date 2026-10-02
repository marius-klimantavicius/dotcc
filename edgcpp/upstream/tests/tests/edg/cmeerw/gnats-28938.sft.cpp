//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<typename T> constexpr bool v = true;
  template<typename T> struct B { };
  struct C {
    template<typename T, bool = v<T>> operator T();
  };
  template<typename T> concept X = requires (T t) { B{t}; };
  static_assert(!X<C>);
}

namespace variable_template
{
  template<typename T>
  constexpr bool v = true;

  template<typename T>
  struct B { };

  struct C
  {
    template<typename T, bool = v<T>>
    operator T();
  };

  template<typename T>
  concept X = requires (T t) { B{t}; };

  static_assert(!X<C>);
}

namespace variable_template_fn_call
{
  template<typename T>
  constexpr bool v = true;

  struct B { };

  template<typename T, bool = v<T>>
  void f();

  template<typename T>
  concept X = requires (T t) { f<typename T::type>(); };

  static_assert(!X<B>);
}

namespace function_template
{
  template<typename T>
  constexpr bool f() { return true; }

  template<typename T>
  struct B { };

  struct C
  {
    template<typename T, bool = f<T>()>
    operator T();
  };

  template<typename T>
  concept X = requires (T t) { B{t}; };

  static_assert(!X<C>);
}
