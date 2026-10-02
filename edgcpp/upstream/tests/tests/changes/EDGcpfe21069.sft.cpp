//type:fn
//options_all:--c++11 --strict
//remark:[5.1] Narrowing conversion not correctly diagnosed
// 3/28/19  [EDGcpfe/21069]
//
// Narrowing conversion not correctly diagnosed
//
// The front end previously did not treat a conversion from a signed integer type
// to a larger unsigned integer type as a narrowing conversion (in C++11 and later
// modes).
//
// That is now fixed.
unsigned long long x[] = { -1 };  // Previously a warning in strict C++11
                                  // mode.  Now an error.
