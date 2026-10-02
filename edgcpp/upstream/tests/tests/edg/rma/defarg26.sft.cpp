//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn: --diag_warning=949;fp

extern "C" void printf(char *, ...);
void f(int i, int j) {
  printf("::f()\ti = %d, j = %d\n", i, j);
}
typedef void (T)(int,int=0);
T *pf = f;
struct A {
  void f(int i, int j) {
    printf("A::f()\ti = %d, j = %d\n", i, j);
  }
} *pa;
void (A::*pma)(int,int=0) = &A::f;
void g(void (*pf)(int,int=0)) {
  (*pf)(0);
}
int main() {
 (*pf)(0);
 (pa->*pma)(0);
 g(&f);
}

