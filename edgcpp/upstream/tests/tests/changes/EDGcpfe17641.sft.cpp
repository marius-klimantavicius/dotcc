//type:fp
//remark:[4.13] Default function call arguments followed by parameter packs
// 12/12/16 [EDGcpfe/17641]
//
// Default function call arguments followed by parameter packs
//
// The front end previously issued a spurious error when redeclaring a function
// template whose original declaration includes a parameter with a default
// argument followed by a parameter pack.
//
// This is now fixed.
template<typename... T> void g(int i = 0, T... ps);
template<typename... T> void g(int i, T... ps);
  // Previously, an error was issued for the redeclaration.  Now okay.
