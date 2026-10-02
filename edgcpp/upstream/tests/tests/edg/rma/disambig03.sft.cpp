//options_all:-r -x -tused
//options: --strict;cn

extern void chk(int);
void f() {
  chk(sizeof(int (*)() const) > 0);
  chk(sizeof(int (*)() volatile) > 0);
  chk(sizeof(int (*)() volatile const) > 0);
}

