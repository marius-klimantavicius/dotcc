//type:fp
//options_all:--c++14
//remark:[4.14] Abort in generic_cast_constant after local deduced-return type declaration
// 4/25/17  [EDGcpfe/18258]
//
// Abort in generic_cast_constant after local deduced-return type declaration
//
// In some elaborate examples, the front end could abort with an internal error
// in generic_cast_constant after processing a local declaration of a function
// with a deduced return type.
//
// This is now fixed.
auto f() try {
  throw 42;
} catch (...) {
  auto f();
  return [x = [](auto y){ return y; }](auto z){ return x (z); }(42);
}
int main() {
  int x = f();  // Previously triggered an internal error.
}
