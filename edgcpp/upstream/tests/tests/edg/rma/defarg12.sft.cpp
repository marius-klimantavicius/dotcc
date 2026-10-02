//options_all:-r -x -tused
//options: --strict;cp

void f(int);
void g() {
  void f(int);
  void ff(int);
  {
    void f(int);
  }
}

