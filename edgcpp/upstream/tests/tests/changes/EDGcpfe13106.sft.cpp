//type:fp
//options_all:--microsoft
//remark:[4.5] Spurious error on lambda capturing "this" in Microsoft mode
// 8/20/12  [EDGcpfe/13106]
//
// Spurious error on lambda capturing "this" in Microsoft mode
//
// In some expression contexts involving a lambda expression capturing "this",
// the front end sometimes mis-parsed certain constructs in Microsoft mode.
//
// This is now fixed.
struct S {
  void f(int x) {
    (void)([x, this]()->void {} );  // Previously triggered spurious
  }                                 // "expected a type specifier" error
};                                  // in some Microsoft modes.
