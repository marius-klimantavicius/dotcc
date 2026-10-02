//type:fp
//options_all:--c++14
//remark:[6.0] Invalid treatment of types of lvalues in interpreter
// 8/5/19   [EDGcpfe/19157,EDGcpfe/20085,EDGcpfe/21466,EDGcpfe/21635]
//
// Invalid treatment of types of lvalues in interpreter
//
// The constexpr interpreter previously checked that it could compute the size of
// the types of lvalues.  This could result in spurious errors in cases of lvalues
// referring to objects whose type is incomplete or too large.
//
// Here, the offsetof expression was previously not evaluated as a constant
// because the interpreter considered U to be too large of a type to allocate.
// However, no object of type U has to be allocated to compute the offsetof
// expression.  Here is an example involving an incomplete type:
//
// In this second case, the interpreter failed while attempting to compute the
// size of arr (as part of the evaluation of &arr[1]) while the array length is
// still not known.  (The changes for EDGcpfe/19246 -- in version 5.0 --
// exacerbated this problem by handling more cases with the interpreter.)  This
// issue is now fixed.
#define offsetof(T, M) (__INTADDR__(&(((T*)0)->M)))
union U {
  unsigned char x[(500*1024)];
};
constexpr auto r = offsetof(U, x);  // Previously an error.  Now okay.
