//options_all:-r -x -tused
//options: --microsoft -n;cn

struct B { int i; };
struct S : B {
  S();
private:
  int a, b, c[];
};
struct T : S { };

