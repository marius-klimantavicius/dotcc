//type:fp
//options_all:--microsoft
//remark:[4.8] Microsoft compatibility: Parameter with array type in for-each loop
// 6/14/13  [EDGcpfe/13794]
//
// Microsoft compatibility: Parameter with array type in for-each loop
//
// Microsoft allows a parameter with an array type as the collection variable
// in a for-each loop, and now so do we.
void f(int x[10]) {
  for each(int i in x) {}
}
