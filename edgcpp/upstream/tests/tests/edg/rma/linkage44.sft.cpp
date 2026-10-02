//options_all:-r -x -tused
//options: --strict;cn:;cp

// EDGma00015
static int f();
int g() {
  struct X {
    friend int f();
  };
  return f();
}
int f() { return 0; }

