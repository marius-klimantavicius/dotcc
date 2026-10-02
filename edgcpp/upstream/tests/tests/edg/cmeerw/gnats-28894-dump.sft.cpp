//type:fp
//options_all:--gn 150300 --c++ -d-dump_builtins --set_flag preload_builtin_functions
//options:--target linux_x86_64:--target linux_i686:--target linux_aarch64:--target linux_armv7:--target linux_riscv64:--target linux_riscv32
//filter:sort

#if __riscv
#pragma riscv intrinsic "vector"
#endif
