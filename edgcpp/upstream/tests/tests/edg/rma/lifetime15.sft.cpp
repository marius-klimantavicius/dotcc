//options_all:-r -x -tused
//options: --strict;cp

void f()
{
  {
    int i = 0;
zero:
    try { throw i; }
    catch (int k) {
      if (++i < 2) goto zero;
    }
  }
}

