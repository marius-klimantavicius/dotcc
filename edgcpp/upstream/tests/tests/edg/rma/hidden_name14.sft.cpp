//options_all:-r -x -tused
//options: --strict;cp:;ln

int f();
int main() {
  int f = 1;
  class C {
    friend int ::f();
    void g(int i = ::f());
  };
  if (f) {
    ::f();
  }
}

