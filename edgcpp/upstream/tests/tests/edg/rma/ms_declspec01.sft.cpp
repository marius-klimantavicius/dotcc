//options_all:-r -x -tused
//options: --microsoft_version=1200 -n;cn

__declspec(dllexport) void g1();
__declspec(dllexport) void g2();
__declspec(dllexport) void g3();
__declspec(dllexport) void g4();

class __declspec(dllimport) X {
  void f1();                                    // dllimport
  void f2() { };                                // nothing
  __declspec(dllexport) void f3();              // warning, dllimport
  __declspec(dllexport) void f4() { };          // warning, nothing
  friend void g1();                             // ??
  friend void g2() { };                         // ??
  friend __declspec(dllexport) void g3() { };   // ??
  friend __declspec(dllexport) void g4() { };   // ??
  static int v;
};

class __declspec(dllexport) Y {
  void f1();
  void f2() { };
  __declspec(dllexport) void f3();              // warning, dllexport
  __declspec(dllexport) void f4() { };          // warning, dllexport
  __declspec(dllimport) void f5();              // warning, dllexport
  __declspec(thread) void g() { }
  __declspec(naked) void gg() { }
};

