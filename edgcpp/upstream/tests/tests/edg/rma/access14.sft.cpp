//options_all:-r -x -tused
//options: --strict;cn:;cn

struct S {
private:
  typedef int I;
  typedef int J;
  struct N;
  struct N2 { };
public:
  typedef int I;
  struct N { };
  struct N2;
};
main() {
  S::I i;
  S::J j;
  S::N x;
  S::N2 y;
}

