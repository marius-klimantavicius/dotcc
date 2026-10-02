//options_all:-r -x -tused
//options: --strict;cn:;ln

extern "C" int printf(char *, ...);
typedef void FUNC();
extern "C" typedef void FUNC_c();
void f(FUNC*) { printf("C++ function"); }
void f(FUNC_c*) { printf("C function"); }
extern FUNC *pf;
extern FUNC_c *pfc;
main() {
  f(pf);
  f(pfc);
}


