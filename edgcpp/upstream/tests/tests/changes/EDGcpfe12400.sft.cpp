//type:fp
//remark:[4.4] Abort on selective overriding of __interface members
// 11/3/11  [EDGcpfe/12400]
//
// Abort on selective overriding of __interface members
//
// In some Microsoft-mode situations where a derived class selectively overrides
// member functions from distinct indirectly-inherited __interface base classes,
// the front end could abort with an internal error in interface_slot_override
// (in class_decl.c).
//
// This is now fixed.
__interface B1 { virtual void f1() = 0; };
__interface B2 { virtual void f2() = 0; };
__interface C: B1, B2 {};
struct D: C {
  void C::f1() {}
  void C::f2() {}  // Triggered an internal error.
};
