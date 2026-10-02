//options_all:-r -x -tused
//options: --strict;cp

// Case involving virtual base classes that shows that access
// is influenced by friendship at intermediate steps.
struct A { int i; };
struct B : virtual private A { friend void f(); };
struct BB : virtual private A { friend void fb(); };
struct C : virtual private B, virtual private BB {
  friend void f();
  friend void fb();
};
void f() {
  C *p = new C;
  p->i = 1;  // Okay because of friendship on C-->B-->A
}
void fb() {
  C *p = new C;
  p->i = 1;  // Okay because of friendship on C-->BB-->A
}

