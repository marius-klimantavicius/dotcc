//type:fp
//options:--c++20 --clang_version 180100

void f()
{
  __builtin_unreachable();
  static_assert(noexcept(__builtin_unreachable()));
}
