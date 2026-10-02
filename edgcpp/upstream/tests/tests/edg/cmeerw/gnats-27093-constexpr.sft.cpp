//type:fp
//options:--c++20 --clang_version 180100 --target linux_aarch64

constexpr bool f()
{
  __attribute__((neon_vector_type(4))) unsigned short v = { };
  v = v + 1;

  __attribute__((neon_polyvector_type(4))) unsigned short p = { };
  p = p + 1;

  return true;
}

static_assert(f());
