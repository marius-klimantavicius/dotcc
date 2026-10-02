//type:fp
//options_all:--g++
//remark:[6.8] GNU/Clang compatibility: incorrect results when folding __builtin_ceil
// 2/20/25  [EDGcpfe/27972]
//
// GNU/Clang compatibility: incorrect results when folding __builtin_ceil
//
// The changes for EDGcpfe/24997 (in version 6.4) allowed for folding of the
// ceil* family of builtins under certain circumstances.  The algorithm used for
// folding values in the range [-0.0..1.0) had led to incorrect results and is
// now fixed.
extern "C" double ceil(double);
static_assert(ceil(0.5) == 1.0);
