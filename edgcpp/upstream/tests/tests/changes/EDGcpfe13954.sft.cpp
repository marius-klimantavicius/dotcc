//type:fp
//remark:[4.7] Spurious error on field with volatile member in union
// 5/1/13   [EDGcpfe/13954]
//
// Spurious error on field with volatile member in union
//
// Version 4.6 introduced a bug that caused a spurious error ("disallowed
// member function") to be issued on certain union type definitions containing
// a field of a class type that itself contains a volatile field.
//
// This is now fixed.
struct T {};
struct S { volatile T t; };
union U {
  S s;  // Previously triggered a spurious error indicating that S
};      // has a disallowed member function.
