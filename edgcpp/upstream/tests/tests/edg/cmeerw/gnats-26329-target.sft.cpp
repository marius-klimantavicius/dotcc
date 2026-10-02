//type:fp
//options:--target linux_aarch64:--target linux_armv7
//options_all:--c++17 --clang_version 180100

# 1 "/usr/lib/llvm-18/lib/clang/18/include/arm_neon.h" 3 4
__attribute__((target("bf16"))) void f()
{ }

void g()
{
  f();
}
