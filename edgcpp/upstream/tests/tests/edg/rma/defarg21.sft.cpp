//options_all:-r -x -tused
//options: --strict;cn:;ln

struct S {
  S(const S&);
};
//typedef int I;
S g(int, int=0);
S g(int=0, int);
main() {
  S s = g();
}

