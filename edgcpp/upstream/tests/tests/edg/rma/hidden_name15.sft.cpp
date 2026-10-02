//options_all:-r -x -tused
//options: --strict;cp:;ln

int f();
int main() {
  extern int f();
  class C {
    friend int ::f();
  };
  if (f()) {
    (void)::f();
  }
}

