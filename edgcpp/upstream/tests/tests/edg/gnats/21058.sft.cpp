//options_all:--clang
int __cdecl func() { return 0; }
int __fastcall func2() { return 0; } // C (>=3.2), or C++ (>=3)
int __stdcall func3() { return 0; }
int __thiscall func4() { return 0; }
int __vectorcall func5() { return 0; } // >=3.6
int * func10() { return __nullptr; } // C++ (>= 3)
