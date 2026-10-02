//type:fn
//remark:[5.0] Static data member template with "auto" type specifier
// 2/9/18   [EDGcpfe/19307]
//
// Static data member template with "auto" type specifier
//
// In some cases involving a data member template declared with an "auto" type
// specifier, the front aborted in prescan_initializer_for_auto_type_deduction.
//
// That is now fixed.
struct A {
  template <class T> static auto const x = 42;  // Previously aborted.
};                                              // Now okay.
