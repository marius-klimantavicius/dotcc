//type:fp
//options_all:--c++14
//remark:[6.8] Infinite loop on code with mismatched "diagnostic push/pop" directives
// 2/26/25  [EDGcpfe/27991]
//
// Infinite loop on code with mismatched "diagnostic push/pop" directives
//
// In code that has mismatched "diagnostic pop" entries (i.e., that don't match
// a preceding "diagnostic push"), an infinite loop could occur when trying to
// determine if a diagnostic should be issued.
_Pragma("diagnostic pop");
_Pragma("diag_suppress deprecated_entity");
struct [[deprecated]] A {
  _Pragma("diagnostic pop");
};
A a;
