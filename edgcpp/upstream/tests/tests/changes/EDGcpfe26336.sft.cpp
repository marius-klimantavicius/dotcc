//type:fp
//options_all:--c++20
//remark:[6.6] Partial ordering of constrained function templates with trailing parameter pack
// 7/28/23  [EDGcpfe/26336]
//
// Partial ordering of constrained function templates with trailing parameter pack
//
// Although function template #1 is more specialized than #2 according to the
// rules in [temp.deduct.partial] due to #2 having a trailing function parameter
// pack, the front end previously considered constraints, and as neither is more
// constrained than the other, the call was treated as ambiguous.  That is now
// fixed.
template<typename T> requires true
int f(T);         // #1
template<typename T, typename ... U> requires true
int f(T, U ...);  // #2
int i = f(1);  // Previously ambiguous.  Now okay.
