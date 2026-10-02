//options_all:-r -x -tused
//options: --strict;cn

// Taken from secion 11.3 of the draft standard (4/14/91)
class A {
public:
  int z;
  int z1;
};
class B : public A {
  int a;
public:
  int b, c;
  void bf();
protected:
  int x;
  int y;
};
class D : private B {
  int d;
public:
  B::c;
  B::z;
  A::z1;
  int e;
  void df();
protected:
  B::x;
  int g;
};
class X : public D {
  void xf();
};
void ef() {
  D d;
  d.z = 0;   // okay
  d.z1 = 0;   // okay
  d.a = 0;   // access error
  d.b = 0;   // access error
  d.c = 0;   // okay
  d.x = 0;   // access error
  d.y = 0;   // access error
  d.d = 0;   // access error
  d.e = 0;   // okay
  d.g = 0;   // access error
  d.bf();   // access error
  d.df();   // okay
}
void D::df() {
  z = 0;   // okay
  z1 = 0;   // okay
  a = 0;   // access error
  b = 0;   // okay
  c = 0;   // okay
  x = 0;   // okay
  y = 0;   // okay
  d = 0;   // okay
  e = 0;   // okay
  g = 0;   // okay
  bf();   // okay
  df();   // okay
}
void B::bf() {
  a = 0;   // okay
  b = 0;   // okay
  c = 0;   // okay
  z = 0;   // okay
  z1 = 0;   // okay
  bf();   // okay
  x = 0;   // okay
  y = 0;   // okay
}
void X::xf() {
  z = 0;   // okay
  z1 = 0;   // okay
  a = 0;   // access error
  b = 0;   // access error
  c = 0;   // okay
  x = 0;   // okay
  y = 0;   // access error
  d = 0;   // access error
  e = 0;   // okay
  g = 0;   // okay
  bf();   // access error
  df();   // okay
}
void ff() {
  X x;
  x.z = 0;   // okay
  x.z1 = 0;   // okay
  x.a = 0;   // access error
  x.b = 0;   // access error
  x.c = 0;   // okay
  x.x = 0;   // access error
  x.y = 0;   // access error
  x.d = 0;   // access error
  x.e = 0;   // okay
  x.g = 0;   // access error
  x.bf();   // access error
  x.df();   // okay
}

