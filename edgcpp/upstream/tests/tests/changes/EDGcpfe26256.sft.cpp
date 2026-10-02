//type:fn
//options_all:--c++17 --gnu_version=90400
//remark:[6.5] Abort on ambiguous user-defined conversion
// 4/18/23  [EDGcpfe/26256]
//
// Abort on ambiguous user-defined conversion
//
// The changes for EDGcpfe/22154 introduced a regression in version 6.3 causing
// some ambiguous conversion cases to lead to an abort due to a null pointer
// indirection in overload.c (function match_with_udc_to_constructor_class).
//
// That is now fixed.
struct S {
  S(float);
  S(short);
};
struct R {
  operator float() const;
  operator double() const;
};
void g() {
  R s;
  static_cast<S>(s);  // Previously aborted.  Now an ordinary error.
}
