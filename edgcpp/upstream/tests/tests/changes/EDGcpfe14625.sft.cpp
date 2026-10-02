//type:fp
//options_all:--microsoft
//remark:[4.9] Microsoft compatibility: floating-point literals in constant expressions
// 2/22/14  [EDGcpfe/14625]
//
// Microsoft compatibility: floating-point literals in constant expressions
//
// The changes for constexpr support in version 4.6 inadvertently caused a
// regression preventing the use of floating-point literals in constant
// expressions in Microsoft mode.  This is now fixed.
void f() {
  static_assert(1.0 < 2.0, "err");  // Previously an error in Microsoft mode
}
