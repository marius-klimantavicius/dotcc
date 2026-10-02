//options_all:-r -x -tused
//options: --microsoft_version=1200 -n;cn

class A {
public:
  __declspec(dllexport) A(int);
  __declspec(dllexport) A();
};
A a;
A a2(0);

class __declspec(dllexport) B {
public:
  __declspec(dllexport) B();
  __declspec(dllimport) B(int);
  B(A&);
} b;

