//type:fp
//options_all:--c++11 --clang
//remark:[4.12] GNU/Clang compatibility: Folding of __builtin_signbit
// 8/3/16   [EDGcpfe/17340]
//
// GNU/Clang compatibility: Folding of __builtin_signbit
//
// In GNU and Clang modes the front end now folds calls to __builtin_signbit (and
// __builtin_signbitf and __builtin_signbitl) for constant operands.  (Like Clang,
// but unlike GCC, the front end will not fold those functions if the operand is
// "not a number".)
static_assert(__builtin_signbit(-1.0), "Unexpected");
  // Now accepted in GNU and Clang C++11 modes.
