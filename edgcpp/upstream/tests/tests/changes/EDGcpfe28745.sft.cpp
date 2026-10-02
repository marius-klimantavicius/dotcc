//type:fp
//options_all:--c11 --gnu_version=100500 --gcc
//remark:Address of C11 _Generic lvalue
// 3/18/26  [EDGcpfe/28745]
//
// Address of C11 _Generic lvalue
//
// In some configurations, the front end was unable to "see through" a C11
// _Generic selection to identify a constant address.  That could result in
// spurious errors.
//
// That is now fixed.
struct S* arr[1];
struct S** p = _Generic(arr, struct S**: arr);  // Previously an error in
                                                // some configurations.
