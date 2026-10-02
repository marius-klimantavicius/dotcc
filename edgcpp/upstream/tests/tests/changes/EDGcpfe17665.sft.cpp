//type:fp
//options_all:--microsoft
//remark:[4.14] Microsoft compatibility: this->enumerator
// 3/15/17  [EDGcpfe/17665]
//
// Microsoft compatibility: this->enumerator
//
// In Microsoft mode, accessing an enumerator constant e with the form "this->e"
// is now treated as a valid constant-expression.
struct S {
  enum { e = 42 };
  void f() {
    static_assert(this->e, "");  // Now accepted in Microsoft mode.
  }
};
