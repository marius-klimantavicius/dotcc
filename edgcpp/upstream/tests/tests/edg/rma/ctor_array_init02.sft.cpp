//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;rp

int ivalue(int i) { return i; }
extern "C" int printf(char *, ...);
void ieq(int i, int j) { printf("%d %s %d\n", i, (i==j) ? "==" : "!=", j); }

struct B {
  int i;
  static int ctor, dtor;
  B(int ii) : i(ii) { ieq(++ctor, ivalue(i)); }
  ~B() { ieq(dtor--, ivalue(i)); }
};
int B::ctor = 0, B::dtor = 0;

struct D : public B {
  D(int ii) : B(ii) { }
};

struct X {
  D d;
  X(int ii) : d(ii) { }
};
X x[] = { 1, 2, 3 };
X y = 4;

int main()
{
  B::dtor = B::ctor;
}

