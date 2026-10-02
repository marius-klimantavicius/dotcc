//options_all:-r -x -tused
//options: --microsoft_version=1200 -n;cn

extern "C" {
  __declspec(dllimport) void f();
};
void g();
void g(int);
extern "C" __declspec(dllimport) void f();
__declspec(dllimport) extern "C" void f();
void g();
extern "C++" void g(int);

extern "C" {
  struct __declspec(dllimport) X {
    void f();
    void g();
  };
}
__declspec(dllimport) extern "C" void X::f() { }
extern "C" __declspec(dllimport) void X::g() { }

