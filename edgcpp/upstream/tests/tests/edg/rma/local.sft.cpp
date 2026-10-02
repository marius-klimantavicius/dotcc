//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:--diag_warning=260;cp

extern f() {
  goto XX;
  class A { int g() { return 0; } };
  return;
XX:;
}
extern g() {
  return;
}

