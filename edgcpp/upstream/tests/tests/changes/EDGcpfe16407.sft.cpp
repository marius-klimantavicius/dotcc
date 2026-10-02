//type:fp
//remark:[4.11] Abort on increment/decrement of volatile property
// 8/15/15  [EDGcpfe/16407]
//
// Abort on increment/decrement of volatile property
//
// In Microsoft mode, an increment or decrement operation applied to a property
// could result in an internal error (in cast_operand_full, "operand is not a
// prvalue") if the "get" accessor of the property produces a volatile lvalue.
//
// This is now fixed.
struct A {
  volatile long m;
  __declspec(property(get=p_get, put=p_put)) volatile long p;
  volatile long& p_get() { return m; }
  volatile long& p_put(volatile long const &x) { m = x; return m; };
  void f() { p++; }  // Previously triggered an internal error.
};
