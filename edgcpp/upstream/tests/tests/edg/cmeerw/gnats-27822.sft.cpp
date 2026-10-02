//type:fp
//options:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1942

namespace minimal
{
  template<typename>
  struct B {
    template<typename T>
    struct N { using A = T; };
  };
  template<typename T> concept C1 = sizeof(T) != 0;
  template<typename T> concept C2 = C1<typename B<T>::template N<T>::A>;
  template<typename T> bool f() requires C2<T>;
  template<typename T> bool f() requires C2<T> && true;
  bool b = f<int>();
}

namespace pr_1
{
  struct X {
    typedef int type;
  };

  template<class>
  struct Xwrapper {
    template<class>
    using type = X;
  };

  template <class T>
  using X2 = Xwrapper<T>::template type<int>::type;

  template<class, class>
  concept C1 = true;

  template<class T>
  concept C2 =
  C1<T, int>
  // Putting X2<T> first changes assertion to NULL arg list
  // Putting int instead of T as first parameter makes this work
  && C1<T, X2<T>>
  && true;

  template<class T>
  requires C1<T, int>
  constexpr bool func() {
    return false;
  }

  template<class T>
  // Replacing C2<T> with the contents of C2 makes this work
  requires C2<T>
  constexpr bool func() {
    return true;
  }

  static_assert(func<int>());
}

namespace pr_2
{
  struct X {
    typedef int type;
  };

  template<class>
  struct Xwrapper {
    template<class>
    using type = X;
  };

  template <class T>
  using X2 = Xwrapper<T>::template type<int>::type;

  template<class, class>
  concept C1 = true;

  template<class T>
  concept C2 =
  C1<T, int>
  // Putting X2<T> first changes assertion to NULL arg list
  // Putting int instead of T as first parameter makes this work
  && C1<X2<T>, T>
  && true;

  template<class T>
  requires C1<T, int>
  constexpr bool func() {
    return false;
  }

  template<class T>
  // Replacing C2<T> with the contents of C2 makes this work
  requires C2<T>
  constexpr bool func() {
    return true;
  }

  static_assert(func<int>());
}

namespace subsumes_1
{
  template<class>
  struct X {
    template<class>
    struct Y
    {
      using Z = int;
    };
  };

  template<class T, class U>
  concept C1 = sizeof(T) != 0 && sizeof(U) != 0;

  template<class T>
  concept C2 = C1<T, int> && C1<typename X<T>::template Y<int>::Z, T>;

  template<class T>
  concept C3 = C1<T, int> && C1<typename X<T>::template Y<int>::Z, T>;

  template<class T>
  requires C2<T>
  constexpr bool func() {
    return false;
  }

  template<class T>
  requires C3<T> && true
  constexpr bool func() {
    return true;
  }

  static_assert(func<int>());
}

namespace subsumes_2
{
  template<class>
  struct X {
    template<class>
    struct Y
    {
      using Z = int;
    };
  };

  template<class T, class U>
  concept C1 = sizeof(T) != 0 && sizeof(U) != 0;

  template<class T>
  concept C2 = C1<T, int> && C1<typename X<T *>::template Y<int>::Z, T>;

  template<class T>
  concept C3 = C1<T, int> && C1<typename X<T *>::template Y<int>::Z, T>;

  template<class T>
  requires C2<T>
  constexpr bool func() {
    return false;
  }

  template<class T>
  requires C3<T> && true
  constexpr bool func() {
    return true;
  }

  static_assert(func<int>());
}
