//type:fp
//remark:[4.9] Spurious error on indirect field with volatile member in union
// 3/6/14   [EDGcpfe/14914]
//
// Spurious error on indirect field with volatile member in union
//
// The changes for EDGcpfe/13954 did not completely address the problem they
// intended to solve.  Specifically, in C++03 mode, the front end still issued
// a spurious error on a union containing a class type that indirectly includes
// a volatile class-type field (the original problem had been identified for
// direct fields).
//
// This is now fixed.
struct T {};
struct V { volatile T t; };
struct S { V v; };
union U {
  S s;  // Previously triggered a spurious error in C++03 mode; now okay.
};
