//type:fn
//options:--target linux_x86_64:--target linux_i686:--target linux_aarch64:--target linux_armv7
//options_all:-w --c++11 --clang_version 200100

namespace check_types
{
  void f(float f, float __attribute__((__vector_size__(16))) vf,
      int i, int __attribute__((__vector_size__(16))) vi)
  {
    __builtin_elementwise_abs(); // error
    __builtin_elementwise_max(); // error
    __builtin_elementwise_fma(); // error

    __builtin_elementwise_abs(vf);
    __builtin_elementwise_max(vf); // error
    __builtin_elementwise_fma(vf); // error

    __builtin_elementwise_abs(vf, vf); // error
    __builtin_elementwise_max(vf, vf);
    __builtin_elementwise_max(vf, vi); // error
    __builtin_elementwise_fma(vf, vf); // error

    __builtin_elementwise_abs(vf, vf, vf); // error
    __builtin_elementwise_max(vf, vf, vf); // error
    __builtin_elementwise_fma(vf, vf, vf);
    __builtin_elementwise_fma(vf, vi, vf); // error
    __builtin_elementwise_fma(vf, vf, vi); // error

    __builtin_elementwise_abs(vf, vf, vf, vf); // error
    __builtin_elementwise_max(vf, vf, vf, vf); // error
    __builtin_elementwise_fma(vf, vf, vf, vf); // error
  }
}
