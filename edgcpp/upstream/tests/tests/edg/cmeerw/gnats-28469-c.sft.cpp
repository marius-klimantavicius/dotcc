//type:fp
//options:--c99 -w

enum E { E1 = 1 };

void f()
{
  int i = 1;
  int j = E1 - i;
  int k = i - E1;
}
