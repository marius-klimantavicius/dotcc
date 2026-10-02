//options_all:-r -x -tused
//options: --strict;cp

class A {
  int i;
  A& operator+(A&);
  A& operator+(int);
  friend A& operator+(int, A&);
} a;
class B {
  B& operator+(B&);
  B& operator+(int);
  friend B& operator+(int, B&);
} b;
class C {
  C& operator+(C&);
  C& operator+(int);
  friend C& operator+(int, C&);
} c;


