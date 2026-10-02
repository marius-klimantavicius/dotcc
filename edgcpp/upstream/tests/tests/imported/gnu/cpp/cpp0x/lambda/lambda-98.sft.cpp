//type: fp
//options:  --c++03
// PR c++/46159
// { dg-options -std=c++98 }

void
f()
{
  int **p = new(int(*[2]));
}
