//type:fn
//remark:[4.13] Explicit inheriting constructors
// 12/15/16 [EDGcpfe/17821]
//
// Explicit inheriting constructors
//
// The front end previously failed to inherit the "explicit" character of a
// base constructor when synthesizing an inheriting constructor.
//
// This is now fixed.
struct B { explicit B(int); };
struct D: B { using B::B; };
D d = 1;  // Previously accepted.  Now an error.
