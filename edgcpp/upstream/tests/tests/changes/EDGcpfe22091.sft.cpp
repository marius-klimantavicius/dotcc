//type:fp
//options_all:--c++17
//remark:[6.1] Failure to apply copy elision with the conditional operator
// 1/24/20  [EDGcpfe/22091]
//
// Failure to apply copy elision with the conditional operator
//
// The front end was failing to apply copy elision in applicable modes such as
// C++17 (guaranteed copy elision) when an operand to the conditional operator
// contained the comma operator.  This could lead to spurious errors in cases
// where calling the copy/move constructor would result in an error.
//
// This is now fixed.
struct S
{
  S(int);
  S(S&& param)=delete;
};
void func(bool test)
{
  test ? (S(1)) : (1,S(2)); // Spurious "call to deleted move ctor" error
}
