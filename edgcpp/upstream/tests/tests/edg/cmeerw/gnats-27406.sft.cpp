//type:fn
//options:--c++20:--c++20 --gn 140100:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<typename T> struct C {
    C(int);
  };
  template<typename T> C(T) -> const C<void>; // error (non-GCC/MSVC)
  template<typename T> using A = C<T>;
  A a{1};
}

namespace tmpl_cv_qual
{
  template<typename T> struct C
  {
    C(int);
  };

  template<typename T>
  C(T) -> const C<void>;        // error (non-GCC/MSVC)

  template<typename T>
  using A = C<T>;

  A a{1};
}

namespace non_tmpl_cv_qual
{
  template<typename T> struct C
  {
    C(int);
  };

  C(int) -> const C<void>;      // error (non-GCC/MSVC)

  template<typename T>
  using A = C<T>;

  A a{1};
}

namespace tmpl
{
  template<typename T>
  struct C
  {
    C(...);
  };

  template<typename T>
  using A = C<T>;

  template<typename T>
  C(T) -> A<T>;                 // error (non-GCC/MSVC)

  template<typename T>
  C(T &) -> C<T> &;             // error

  int i;
  A ai{i};

  char c;
  A ac{c};
}

namespace non_tmpl
{
  template<typename T>
  struct C
  {
    C(...);
  };

  template<typename T>
  using A = C<T>;

  C(char) -> A<char>;           // error (non-GCC/MSVC)
  C(char &) -> C<char> &;       // error

  char c;
  A ac{c};
}
