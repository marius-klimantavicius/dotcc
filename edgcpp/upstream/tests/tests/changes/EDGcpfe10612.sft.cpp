//type:fp
//remark:[4.5] Conversion of a no-capture lambda to a function pointer
// 1/17/12  [EDGcpfe/10612,EDGcpfe/11770,EDGcpfe/12243]
//
// Conversion of a no-capture lambda to a function pointer
//
// When a lambda expression is introduced with "[]" (i.e., it doesn't capture any
// local variables), its closure type now includes an implicit conversion operator
// to an ordinary function pointer: A call through that pointer is equivalent to
// an invocation of the lambda.
//
// The conversion function returns the address of a special static member function
// representing an alternative entry point to the call operator of the closure.
// Ordinarily, this static member (called "_FUN") is not visible to user code, but
// in GNU C++ mode it is visible since that is how GCC behaves.
auto c = []{ return 42; };
int (*pf)() = c;  // Implicit conversion of closure to function pointer.
int main() {
  pf();  // Same effect as "c();", but via an indirect call.
}
