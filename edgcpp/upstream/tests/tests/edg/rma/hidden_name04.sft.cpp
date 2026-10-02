//options_all:-r -x -tused
//options: --strict;cp

class x { };
void f() {
  int x;
  class x a;
  class ::x b;
  class x { };
  class x c;
}

