//options_all:-r -x -tused
//options: --strict;fp:;fp

asm ("xxx");
void f() {
  asm ("yyy");
  {
    asm ("zzz");
  }
}

