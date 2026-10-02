//type:fn
//options_all:--microsoft
//remark:[4.14] Microsoft-mode abort on cast of xvalue to lvalue reference
// 3/21/17  [EDGcpfe/18133]
//
// Microsoft-mode abort on cast of xvalue to lvalue reference
//
// In Microsoft mode, the front end sometimes aborted with an internal error in
// conv_class_prvalue_operand_to_lvalue when processing a cast of an xvalue to an
// lvalue reference type.
//
// This regression introduced by the changes for EDGcpfe/12021 (version 4.8) is
// now fixed.
struct S {};
S&& f();
S &s = static_cast<S&>(f());  // Previously aborted in Microsoft mode.
                              // Now okay.
