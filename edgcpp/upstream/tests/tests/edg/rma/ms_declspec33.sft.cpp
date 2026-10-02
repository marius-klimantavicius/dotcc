//options_all:-r -x -tused
//options: --microsoft_version=1200 -n;cn

__declspec(dllimport) void f1();
__declspec(dllexport) void f1() { }
__declspec(dllimport) void f2();
void f2() { }
__declspec(dllimport) inline void f3();
void f3() { }
__declspec(dllimport) void f4() { }

