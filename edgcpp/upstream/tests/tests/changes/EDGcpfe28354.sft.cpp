//type:fp
//options_all:--gn 999999
//remark:[6.8] Array designators for arrays of types with nontrivial construction/destruction
// 7/22/25  [EDGcpfe/28354]
//
// Array designators for arrays of types with nontrivial construction/destruction
//
// Array designators were previously not permitted in C++ mode if the designated
// elements may involve nontrivial construction or destruction.  GCC also
// disallows such cases, except it permits designators that have no effect (i.e.,
// designators that do not change the index of the next initializer).  The front
// end now emulates that in GNU C++ modes.
struct X { ~X(); };
X arr[3] = { X{}, [1] = X{} };  // Now okay.  The array designator has no
                                // effect.
