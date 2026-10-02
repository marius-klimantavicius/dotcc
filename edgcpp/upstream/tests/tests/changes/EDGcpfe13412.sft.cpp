//type:fn
//options_all:--microsoft
//remark:[4.6] Abort in Microsoft mode on malformed constructor-like member declaration
// 11/21/12 [EDGcpfe/13412]
//
// Abort in Microsoft mode on malformed constructor-like member declaration
//
// In Microsoft mode, the front end aborted in function is_constructor_decl
// (decl_spec.c) on certain malformed constructor-like member declarations with
// a qualified name (qualified constructor names are sometimes accepted in
// class-scope member declarations in Microsoft mode).
//
// This is now fixed (an error is issued for the malformed declaration).
struct S1 { void S2(); };
struct S2 {
  S1::S2();  // This constructor-like declaration previously triggered an
};           // abort.  Now it just elicits an error.
