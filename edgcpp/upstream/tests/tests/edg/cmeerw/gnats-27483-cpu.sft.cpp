//type:fp
//options:--clang_version 190100 --target linux_x86_64:--clang_version 190100 --target linux_i686:--clang_version 190100 --target linux_aarch64:--clang_version 190100 --target linux_armv7:--clang_version 180100 --target linux_x86_64:--clang_version 180100 --target linux_i686:--clang_version 180100 --target linux_aarch64;fn:--clang_version 180100 --target linux_armv7;fn
//options_all:--c++

void f()
{
    __builtin_cpu_init();
    __builtin_cpu_is("intel");
    __builtin_cpu_supports("mmx");
}
