//options_all:-r -x -tused --diag_suppress=nonstd_member_function_address
//options: --strict;cn:;cn

class A;
void (A::*pf1)();
void (A::*pf2)() throw(int);
void (A::*pf3)() throw();
class A {
  void ff() throw(int);
  void f() {
    pf1 = pf2;       // okay -- pf1 expects anything
    pf1 = pf3;       // okay -- pf1 expects anything, pf3 throws nothing
    pf2 = pf1;       // error -- pf1 expects int, pf2 throws anything
    pf2 = pf3;       // okay -- pf3 throws nothing
    pf3 = pf1;       // error
    pf3 = pf2;       // error
    pf1 = f;
    pf2 = f;         // error
    pf3 = f;         // error
    pf1 = ff;
    pf2 = ff;
    pf3 = ff;        // error
  }
};

