//type:fp
//options:--c:--c++
//options_all:--clang_version 180100 --target linux_aarch64

#ifdef __cplusplus
namespace minimal
{
#endif
  void f() {
    __builtin_sve_svundef2_b();
  }
#ifdef __cplusplus
}
#endif
