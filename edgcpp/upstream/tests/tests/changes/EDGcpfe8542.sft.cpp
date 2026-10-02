//type:fn
//options_all:--c
//remark:[4.9] Static initialization with address constants
// 11/22/13 [EDGcpfe/8542,EDGcpfe/14697]
//
// Static initialization with address constants
//
// The front end now diagnoses as an error an attempt to statically initialize
// with an address constant a variable of field whose size is different from the
// address type.  In the case of bit fields, the field's width is taken into
// account.
typedef __EDG_PTRDIFF_TYPE__ Int;
int x;
struct S { Int bits: 10; } s = { (Int)&x };
  // Now an error in C modes.
