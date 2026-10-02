//options_all:-r -x -tused
//options: --strict;cn

void f();
void g();
void h();
class A {
  friend void ::f();          // Okay
  friend void ::g() { }       // Error
  friend void h() { }         // Okay
};

