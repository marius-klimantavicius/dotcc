//type:fp
//remark:[4.11] Conditional operator and user-defined conversions
// 11/30/15 [EDGcpfe/16675]
//
// Conditional operator and user-defined conversions
//
// When processing a conditional operator the front end follows a number of
// standard rules to find a "common type" for the second and third operands of
// that operator.  Previously, the front end sometimes failed the process when
// at least one of the operands requires a user-defined conversion to a pointer
// type followed by a qualification conversion.
//
// This is now fixed.
struct S { operator volatile void*(); };
void const *p;
auto x = true ? p : S();  // Previously triggered a spurious error.
                          // Now okay: x has type "void const volatile*".
