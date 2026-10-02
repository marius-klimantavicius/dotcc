//type:fn
//options_all:--gn 69999 --c++14 -tused
//remark:[6.0] Pseudo-destructors
// 7/31/19  [EDGcpfe/21397]
//
// Pseudo-destructors
//
// Naming a pseudo-destructor is only permitted as the immediate expression
// designating the target of a call.  The front end previously did not enforce
// that constraint.
//
// That is now fixed.  In addition, diagnostics that previously referred to a
// "vacuous destructor call" are now phrased using the more widely-known
// term "pseudo-destructor call".
using S = int;
int *p;
using X = decltype(p->~S); // Previously accepted.  Now an error.
                           // ("decltype(p->~S())" would be okay.)
