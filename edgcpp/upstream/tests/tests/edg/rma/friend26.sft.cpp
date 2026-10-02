//options_all:-r -x -tused
//options: --strict;cn

// similar to friend25.C, but with functions
void f();
struct S1 {
  void f();
  void g();
};
struct S2 : public S1 {
  friend void f();
  friend void g();
private:
  static int i;
};
void f() { S2::i = 0; }
void S1::f() { S2::i = 0; }
void g() { S2::i = 0; }
void S1::g() { S2::i = 0; }

