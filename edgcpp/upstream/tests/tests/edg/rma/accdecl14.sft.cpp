//options_all:-r -x -tused
//options: --strict;cp

struct A {
  typedef struct s { int m; } t;
  typedef struct ss { int m; } tt;
};

struct C : private A {
public:
  A::s;
  int s;
  int ss;
  A::ss;
};
struct CC : public C {
  void f();
  void g();
};
void CC::f() { s = 0; ss = 0; }
void CC::g() { struct s x; struct ss y; x.m = y.m; }

