//options_all:-r -x -tused
//options: --strict;cp

extern "C" {
extern void foo();
}

void bar()
{
  extern void foo();
  foo();
}


