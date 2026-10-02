//type:fp
//options_all:--g++
//remark:[4.0] GNU and Microsoft compatibility: Abstract class parameters in templates
// 8/21/08  [EDGcpfe/9126]
//
// GNU and Microsoft compatibility: Abstract class parameters in templates
//
// Usually, any attempt to create a parameter type that is an abstract class type
// is diagnosed as an error by the front end.  Now, only a warning is issued in
// GNU and Microsoft C++ modes if the parameter is the result of the declaration
// or instantiation of a function template, and if the abstract-type parameter is
// not a parameter of the function template itself.
struct A { virtual void p() = 0; };
template<typename T> void f(void (*)(T)) {}
int main() {
  f<A>(0);  // Now elicits a warning instead of an error.
}
