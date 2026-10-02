//options_all:-r -x -tused
//options: --microsoft -n;cn

extern "C" int printf(char *, ...);
struct A {
  __declspec(dllimport) inline void f() { printf("in A::f()\n");}
  __declspec(dllimport) inline void g();
};
__declspec(dllimport) inline void A::g() { printf("in A::g()\n");

int main()
{
	A a;
	a.f();
}

