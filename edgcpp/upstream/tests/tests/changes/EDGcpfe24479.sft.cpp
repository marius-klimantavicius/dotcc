//type:fp
//options_all:--c++14
//remark:[6.8] Abort due to error node passed to lowering for nested generic lambda
// 2/27/25  [EDGcpfe/24479]
//
// Abort due to error node passed to lowering for nested generic lambda
//
// Previously, a generic lambda declarator that referred to a parameter of an
// enclosing generic lambda could result in an error node during substitution of
// its conversion operator template.  In configurations that do IL lowering, this
// later triggered an internal error in lower_type.
int (*p)(int) = [] (auto o) {
  return [] (auto i) -> decltype(o + i) { return 0; };
} (1);  // Previously triggered an internal error.  Now okay.
