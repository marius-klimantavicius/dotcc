//type:fn
//options:--c++20:--c++20 --gn 130100:--c++20 --clang_version 160000:--ms_c++20 --microsoft_version 1936:--ms_c++17 --microsoft_version 1927
//options_all:-tused

namespace non_inst
{
  constexpr int ci;

  const int i;

  static const int si;          // accepted by MSVC

  inline const int ii;

  auto v = ci + i + si;


  /* Most of these are accepted by MSVC (except for the explicit
     specialization), but we currently diagnose them in Microsoft mode
     anyway. */

  template<typename T>
  constexpr int tci;            // accepted by GCC

  template<typename T>
  const int ti;                 // accepted by GCC

  template<typename T>
  const int ti<T *>;            // accepted by GCC

  template<>
  const int ti<void>;

  template<typename T>
  inline const int tii;         // accepted by GCC

  template<typename T>
  const T tt;                   // OK

  struct C
  {
    template<typename T>
    static constexpr int sci;

    template<typename T>
    static const int si;        // OK
  };

  template<typename T>
  const int C::si;              // accepted by GCC

  template<typename T>
  struct D
  {
    static constexpr int sci;

    static const int si;        // OK

    template<typename U>
    static constexpr int tsci;

    template<typename U>
    static const int tsi;       // OK
  };

  template<typename T>
  const int D<T>::si;           // accepted by GCC

  template<typename T> template<typename U>
  const int D<T>::tsi;          // accepted by GCC
}

namespace inst
{
  template<typename T>
  constexpr int tci;            // error

  template<typename T>
  const int ti;                 // error

  template<typename T>
  inline const int tii;         // error

  template<typename T>
  const T tt;                   // error

  /* MSVC suppresses some diagnostics for the same class after the first
     error. */

  struct C
  {
    template<typename T>
    static constexpr int sci;   // error

    template<typename T>
    static const int si;        // OK
  };

  template<typename T>
  const int C::si;              // error

  template<typename T>
  struct D
  {
    static constexpr int sci;   // error

    static const int si;        // OK

    template<typename U>
    static constexpr int tsci;  // error

    template<typename U>
    static const int tsi;       // OK
  };

  template<typename T>
  const int D<T>::si;           // error

  template<typename T> template<typename U>
  const int D<T>::tsi;          // error

  auto v = tci<int> + ti<int> + tii<int> + tt<int> +
           C::sci<int> + C::si<int> +
           D<int>::sci + D<int>::si + D<int>::tsci<int> + D<int>::tsi<int>;
}
