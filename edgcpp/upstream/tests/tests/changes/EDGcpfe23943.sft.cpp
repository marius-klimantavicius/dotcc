//type:fp
//options_all:--c++14
//remark:[6.4] Assertion failure in type_is_lambda_in_default_argument
// 3/18/22  [EDGcpfe/23943]
//
// Assertion failure in type_is_lambda_in_default_argument
//
// An assertion failure (in type_is_lambda_in_default_argument) had occurred
// during the mangling of some lambdas that occur in default arguments and is now
// fixed.
struct B {
  virtual ~B() = default;
};
template <typename T> struct D : B { };
template <typename T> D<T> f(const T&) {
  return D<T>();
}
void foo(const B& xxx = f([](){})) {}
