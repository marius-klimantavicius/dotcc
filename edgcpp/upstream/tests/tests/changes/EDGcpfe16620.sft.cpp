//type:fp
//options_all:--c++11
//remark:[4.11] Abort on lambda in default argument
// 11/4/15  [EDGcpfe/16620]
//
// Abort on lambda in default argument
//
// In some cases, a lambda expression in a default argument (in C++11 mode)
// could result in an abort (in type_is_lambda_in_default_argument) during name
// mangling.
//
// This is now fixed.
void g() {
  extern int f(int p = ([]{ int i = 3; return [=]{return i;}; }()()));
}
