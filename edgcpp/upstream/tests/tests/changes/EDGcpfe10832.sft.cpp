//type:fp
//remark:[4.2] Spurious error on initializer expression that starts with a string literal
// 7/14/10  [EDGcpfe/10832]
//
// Spurious error on initializer expression that starts with a string literal
//
// The front end previously sometimes issued a spurious error if an initializer
// expression started with a string literal but was followed by an operation
// (e.g., a subscript).
//
// This is now fixed.
char x[] = { "abc"[1], 0 };  // Previously triggered a spurious error.
