//type:fp
//options_all:--c++17 --g++
//remark:[6.6] Spurious error in GNU mode on aggregate initialization in template
// 8/28/23  [EDGcpfe/26596]
//
// Spurious error in GNU mode on aggregate initialization in template
//
// The changes for EDGcpfe/25550 introduced a regression in version 6.5 of the
// front end, causing this example to elicit a spurious error in GNU modes
// (claiming excess initializer elements).  That is now fixed.
struct B { int x, y, z; };
struct D: B {};
template<typename T> int f(T) {
  D d = { 1, 2, 3 };  // Previously an error.  Now okay.
  return d.y;
}
int main() {
  f(42);
}
