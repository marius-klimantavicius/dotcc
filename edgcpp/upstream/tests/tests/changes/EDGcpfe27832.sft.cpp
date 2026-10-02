//type:fp
//options_all:--clang_version=180000 --c++20
//remark:Constant evaluation of null pointer offsets
// 12/24/25 [EDGcpfe/27832]
//
// Constant evaluation of null pointer offsets
//
// The front end now accepts some null pointer "offset" computations at compile
// time in Clang C++ modes, particularly in array dimension expressions.
int arr1[(long)(char*)0];  // Previously an error.  Now okay.
struct S { int i; };
int arr2[(((long long)&(((S*)0)->i)) == 0)? 1 : -1];  // Ditto.
