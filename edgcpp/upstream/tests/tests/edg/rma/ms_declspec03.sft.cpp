//options_all:-r -x -tused
//options: --microsoft -n;fn

int __declspec(dllimport) f();
int * __declspec(dllimport) g();
//int * __declspec(dllimport) * __declspec(dllimport) h();
int __declspec(dllimport) * __declspec(dllimport) i();
typedef int __declspec(dllimport) * __declspec(dllimport) T();
//typedef int * __declspec(dllimport) * __declspec(dllimport) TT();
int __declspec(dllimport) * __declspec(dllimport) (x)();
int __declspec(dllimport) * (__declspec(dllimport) y)();
void z(int * __declspec(dllimport)()) {}

