//type:fp
//remark:[4.2] Spurious errors on Microsoft-style parameter attributes
// 10/30/09 [EDGcpfe/10163]
//
// Spurious errors on Microsoft-style parameter attributes
//
// In a function declaration whose first parameter involves a parenthesized
// declarator, and the second parameter starts with Microsoft-style (bracketed)
// attributes, the front end often issued spurious errors (in Microsoft mode).
//
// This is now fixed.
void f(char (&a)[10], [in] int b[]);
  // Previously triggered multiple spurious errors, the first of which
  // suggesting that the type of f is incomplete.
