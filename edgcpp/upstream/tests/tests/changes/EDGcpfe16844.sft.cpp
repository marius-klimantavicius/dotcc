//type:fp
//options_all:--c++11
//remark:[4.11] Accept enumerators as argument to alignas
// 4/4/16   [EDGcpfe/16844]
//
// Accept enumerators as argument to alignas
//
// Previously a spurious error was given when an enumerator was used as the
// argument to the alignas alignment specifier.
enum { sixteen = 16 };
alignas(sixteen) int v = 99;
