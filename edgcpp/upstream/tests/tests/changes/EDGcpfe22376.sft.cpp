//type:fp
//options_all:--diag_error=2361 --c++11 --gnu_version=80300 --parse
//remark:[6.1] Conversion from bool incorrectly treated as narrowing
// 5/22/20  [EDGcpfe/22376]
//
// Conversion from bool incorrectly treated as narrowing
//
// The front end previously treated many conversions from bool to another integer
// type as "narrowing", which is incorrect.
//
// That is now fixed.
unsigned g(bool b) {
  return unsigned{b};  // Previously warned about narrowing.  Now
}                      // silently accepted.
