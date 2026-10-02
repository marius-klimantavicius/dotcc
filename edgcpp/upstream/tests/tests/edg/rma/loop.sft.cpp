//options_all:-r -x -tused
//options: --strict;cn:;cn

struct S {
  int i;
  S& operator->() { return this; }
};
S s;
main() {
  int i = s->i;
}


