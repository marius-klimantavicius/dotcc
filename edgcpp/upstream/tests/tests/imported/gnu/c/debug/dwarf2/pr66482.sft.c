//type: fp
//options: --c17 --strict_gnu
/* { dg-do compile } */
/* { dg-options "-std=gnu17 -O3 -gdwarf" } */

void f(int p) {}
int g() {
  void f();
  g();
  return 0;
}
