//options_all:-r -x -tused
//options: --microsoft -n;cp

struct __declspec(dllimport) X {
  void inline f();
  void g() { }
};
__declspec(dllimport) void X::f() { }
__declspec(dllimport) inline void f() { }

