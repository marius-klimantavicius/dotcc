//type:fp
//remark:[4.4] C-generating back end: abort on reference to variable in containing block
// 6/18/11  [EDGcpfe/11808]
//
// C-generating back end: abort on reference to variable in containing block
//
// C-generating back end configurations could abort (in
// find_local_variable_static_init) on code that takes the address of an
// initialized static variable that is promoted to the global scope from a
// containing block.  This is now fixed.
const void* p;
void f() {
  static const int x[] = { 1, -2, 3, 4 };
  {
    int i;
    p = &x;   // Could abort here
  }
}
