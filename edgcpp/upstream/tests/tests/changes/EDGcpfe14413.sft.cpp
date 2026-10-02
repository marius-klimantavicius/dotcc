//type:fp
//remark:[4.9] Spurious error on function template instance passed to T&& parameter
// 12/12/13 [EDGcpfe/14413,EDGcpfe/14730]
//
// Spurious error on function template instance passed to T&& parameter
//
// The front end previously failed to deduce a call to a function template with a
// parameter of the form T&& (where T is a template parameter) when the
// corresponding argument is an instance of a function template.
//
// This is now fixed.
template<typename T> void f(T&&);
template<typename T> void g();
void h() {
  f(g<int>);  // Previously triggered a spurious error about there not
}             // being a matching "f".
