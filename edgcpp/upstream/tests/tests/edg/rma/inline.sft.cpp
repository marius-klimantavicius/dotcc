//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp

class A {
public:
  int a;
  static int sum(int i) { return (i <= 1) ? i : i + sum(i-1); }
  int sum();
};
int A::sum() { return sum(a); }
int f() {
  A x;
  x.a = 5;
  return x.sum();
}

