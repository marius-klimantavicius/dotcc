//options_all:-r -x -tused
//options: --strict;cp:;cp

void f(int);
void f(char);
void g() {
  extern void f();
  extern void f(int);
  class A {
    friend void f();
    friend void ::f(int);
    friend void ::f(char);
  } a;
}

