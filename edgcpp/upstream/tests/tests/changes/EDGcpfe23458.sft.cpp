//type:fp
//options_all:--c++14 -tused --clang_version 90000 --ms_compatibility
//remark:[6.2] Abort on Microsoft array property subscript with Clang ms-extensions mode
// 10/15/20 [EDGcpfe/23458]
//
// Abort on Microsoft array property subscript with Clang ms-extensions mode
//
// The front end previously could abort with an internal error in
// conv_glvalue_to_prvalue when subscripting a Microsoft array property in Clang
// ms-extensions mode.
//
// This is now fixed.
struct S {
  __declspec(property(get=get_x, put=put_x)) int x[];
  int get_x(int i, int j);
  void put_x(int i, int j, int k);
};
int main() {
  S *p = 0;
  return p->x[1][2];  // Previously triggered an internal error in some
}                     // modes.  Now okay.
