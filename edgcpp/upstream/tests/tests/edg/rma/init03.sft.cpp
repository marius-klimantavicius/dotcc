//options_all:-r -x -tused
//options: --strict;cp

extern int f(int);
static void g()
{
  int a (1);
  int b (a+1);
  int c (f(0));
}

