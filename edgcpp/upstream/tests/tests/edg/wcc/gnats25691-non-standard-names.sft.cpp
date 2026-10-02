//type:cp
//options:--strict:--no_strict:--g++:--microsoft:--clang
//options_all:--c++20

int main() {
  static_assert(__FUNCTION__[0] != '\0');
#if _MSC_VER
  static_assert(__FUNCDNAME__[0] != '\0');
#else /* !_MSC_VER */
  static_assert(__PRETTY_FUNCTION__[0] != '\0');
#endif /* _MSC_VER */
  return 0;
}
