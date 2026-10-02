//options_all:-r -x -tused
//options: --strict;cn:;cp

extern int f();
int g() {
  if (auto int i = f()) { return i; }
  return 0;
}

