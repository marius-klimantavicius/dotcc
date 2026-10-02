//type:fn
//options_all:--c++11
//remark:[4.10] Missing diagnostics on alignas(...) and _Alignas(...) in invalid contexts
// 7/30/14  [EDGcpfe/15304]
//
// Missing diagnostics on alignas(...) and _Alignas(...) in invalid contexts
//
// The front end previously failed to diagnose attempts to use the alignas
// specifier in certain contexts not permitted by the C++11 and C11 standards.
//
// C11 has different constraints;
//
// That is now fixed: A diagnostic is now emitted for such cases.
char alignas(4) buf[100];  // Now an error.  (alignas(4) should precede
                           // "char" or immediately follow "buf".)
