//options_all:-r -x -tused
//options: --microsoft -n;cn

void f(int, ...);
void xxx() {
  void (__stdcall *pf1)(int,...);
  void (__cdecl *pf2)(int,...);
  void (__fastcall *pf3)(int,...);
  void (*pf4)(int,...);
  pf1 = &f;
  pf2 = &f;
  pf3 = &f;
  pf4 = &f;
}
void yyy() {
  void (__stdcall *pf1)(int);
  void (__cdecl *pf2)(int);
  void (__fastcall *pf3)(int);
  void (*pf4)(int);
  pf1 = &f;
  pf2 = &f;
  pf3 = &f;
  pf4 = &f;
}

