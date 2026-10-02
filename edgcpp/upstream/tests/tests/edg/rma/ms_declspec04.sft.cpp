//options_all:-r -x -tused
//options: --microsoft_version=1200 -n;cn

__declspec(dllexport) extern "C" void f();
extern "C" void f();
extern "C" __declspec(dllexport) void f();     // error

extern "C++" void f(int);
__declspec(dllexport) extern "C++" void f(int);
extern "C++" __declspec(dllexport) void f(int);    // error

extern "C++" {
  class X {
    void f();
    void f(int);
    void f(int,int);
  };
}

extern "C++" void X::f() { }
__declspec(dllexport) extern "C++" void X::f(int) {}
extern "C++" void __declspec(dllexport) X::f(int,int) { }  // error

