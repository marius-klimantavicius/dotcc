//options_all:-r -x -tused
//options: --strict;cn:;cn

void f() {
  return;
}

int g(i)
int i;
{
  return i;
}

int h(s,i)
struct S { int i; } s;
int i;
{
  s.i = i;
  return i;
}

