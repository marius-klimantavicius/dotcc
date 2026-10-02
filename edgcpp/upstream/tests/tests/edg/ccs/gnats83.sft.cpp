//type:cp

void f()
{
  int (*p)[10] = new int[20][10];
  delete /*[]*/ p;
}
