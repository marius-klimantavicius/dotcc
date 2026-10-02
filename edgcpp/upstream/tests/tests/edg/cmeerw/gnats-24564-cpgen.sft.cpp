//type:fp
//options:--c++20:--c++20 --gn 130100:--c++20 --clang_version 160000:--ms_c++20 --microsoft_version 1936
//options_all:-tused -w

namespace UNQUAL
{
  template<typename T>
  int i = 1;

  template<typename T>
  int i<T *> = 1;

  template<>
  int i<void> = 1;
}

namespace CONST_QUAL
{
  template<typename T>
  const int i = 1;

  template<typename T>
  const int i<T *> = 1;

  template<>
  const int i<void> = 1;
}

namespace CONST_VOLATILE_QUAL
{
  template<typename T>
  const volatile int i = 1;

  template<typename T>
  const volatile int i<T *> = 1;

  template<>
  const volatile int i<void> = 1;
}

namespace INLINE_UNQUAL
{
  template<typename T>
  inline int i = 1;

  template<typename T>
  inline int i<T *> = 1;

  template<>
  inline int i<void> = 1;
}

namespace INLINE_CONST_QUAL
{
  template<typename T>
  inline const int i = 1;

  template<typename T>
  inline const int i<T *> = 1;

  template<>
  inline const int i<void> = 1;
}

namespace INLINE_CONST_VOLATILE_QUAL
{
  template<typename T>
  inline const volatile int i = 1;

  template<typename T>
  inline const volatile int i<T *> = 1;

  template<>
  inline const volatile int i<void> = 1;
}


struct C
{
  int i{};
  int j{};
};

namespace static_binding
{
  static auto [ i, j ] = C{};
}

namespace static_const_binding
{
  static const auto [ i, j ] = C{};
}

namespace static_const_volatile_binding
{
  static const volatile auto [ i, j ] = C{};
}

namespace binding
{
  auto [ i, j ] = C{};
}

namespace const_binding
{
  const auto [ i, j ] = C{};
}

namespace const_volatile_binding
{
  const volatile auto [ i, j ] = C{};
}
