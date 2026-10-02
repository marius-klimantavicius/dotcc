//type:fp
//options_all:--c++11 --microsoft
//remark:[5.1] Microsoft-mode abort on enum bit-field initialization
// 3/19/19  [EDGcpfe/20927]
//
// Microsoft-mode abort on enum bit-field initialization
//
// The front end previously could abort attempting access through a null pointer
// in some cases involving the initialization of a bit field of enumeration type
// in an overload-resolution context.
//
// That problem is now fixed.
struct S {
  enum E { x, y };
  E e: 1;
};
int main() {
  S s{};
  s = { S::x };  // Previously caused an abort.  Now okay.
}
