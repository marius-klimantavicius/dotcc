//options_all:-r -x -tused
//options: --microsoft -n;cn

// Compile in Microsoft-C mode
void f(x, ...) int x; { }
typedef void __stdcall PF1();
PF1 *pf1;
typedef void __cdecl PF2();
PF2 *pf2;
typedef void __fastcall PF3();
PF3 *pf3;
typedef void PF4();
PF4 *pf4;
int main() {
  pf1 = &f;
  pf2 = &f;
  pf3 = &f;
  pf4 = &f;
  return 0;
}

