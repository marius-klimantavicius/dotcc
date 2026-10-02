//type:fp
//options:-DXTHEADVECTOR:-DVECTOR
//options_all:--gn 150200 --target linux_riscv64 -W

#ifdef XTHEADVECTOR
#pragma riscv intrinsic "xtheadvector"
#else
#pragma riscv intrinsic "vector"
#endif

void f()
{
  __riscv_read_vl();
}
