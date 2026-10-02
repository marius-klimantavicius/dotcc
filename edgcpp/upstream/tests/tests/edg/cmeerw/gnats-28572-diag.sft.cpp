//type:fn
//options:--gn 150200 --target linux_riscv64:--gn 150200 --target linux_riscv32
//options_all:--c++11

#pragma riscv intrinsic "vector"

// warning
#pragma riscv intrinsic "invalid"

// warning
#pragma riscv invalid

void f(vint16m1_t i16m1)
{
  i16m1 + 1;                    // error
}
