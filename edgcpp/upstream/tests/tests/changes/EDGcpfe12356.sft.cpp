//type:fp
//options_all:--g++
//remark:[4.4] GNU compatibility: Empty member declarations with attributes in GNU modes
// 11/22/11 [EDGcpfe/12356]
//
// GNU compatibility: Empty member declarations with attributes in GNU modes
//
// In GNU C++ mode, the front end now accepts an empty class member declaration
// that specifies attributes (the attributes are ignored with a warning).
// Although such declarations were already accepted in GNU C mode, the warning
// for that case has been changed to reflect the fact that the attributes are
// ignored.
struct S {
  __attribute((aligned(32)));  // Now accepted with a warning in GNU C++
};                             // mode.
