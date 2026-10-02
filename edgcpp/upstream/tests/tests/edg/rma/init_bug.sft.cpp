//options_all:-r -x -tused
//options: --strict;rp

/* Crashes the front end if IL Lowering not done */
int fn(void) { return(1); }
int a;
int main()
{
  static int StaticIntFn = (int)fn();
}


