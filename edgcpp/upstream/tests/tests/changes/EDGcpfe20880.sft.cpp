//type:fp
//options_all:--g++
//remark:[6.8] GNU/Clang compatibility: __builtin_isinf* builtins and NaNs
// 5/14/25  [EDGcpfe/20880,EDGcpfe/28176]
//
// GNU/Clang compatibility: __builtin_isinf* builtins and NaNs
//
// The __builtin_isinf* builtins had incorrectly returned true when presented
// with NaN values of suitable types and now return false instead.
// with --g++:
static_assert(__builtin_isinf(__builtin_nanf("")) == false);
static_assert(__builtin_isinf(__builtin_nansf("")) == false);
