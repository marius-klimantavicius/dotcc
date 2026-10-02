//options_all:-r -x -tused
//options: --strict;cn:;cp

// spurious linkage conflict error in -A mode [EDGma00031]
static int f();
void g() {
  struct X { friend int f(); };
}

