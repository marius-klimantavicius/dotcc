//options_all:-r -x -tused
//options: --microsoft_version=1300 -n;cp

enum Boolean { FALSE = 0, TRUE = 1 };
void f(Boolean bool) { }
void g() {
  Boolean bool = FALSE;
  f(bool);
}

