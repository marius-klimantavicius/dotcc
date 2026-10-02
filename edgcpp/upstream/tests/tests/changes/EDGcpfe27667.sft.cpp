//type:fp
//remark:[6.7] Abort in C++-generating back end on diagnostic pragma
// 10/17/24 [EDGcpfe/27667]
//
// Abort in C++-generating back end on diagnostic pragma
//
// This previously triggered an internal error in configurations that record
// source sequence entries (in function r_set_keep_in_il_on_sslist(...), in
// il_walk.c).  That is now fixed.
int f(void)
#pragma diag_default = 123 
{
  return 1U;
}
