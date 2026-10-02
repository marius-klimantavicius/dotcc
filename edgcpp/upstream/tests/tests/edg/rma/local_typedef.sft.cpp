//options_all:-r -x -tused
//options: --strict;cn

template<class T> struct A { T t; };
void f() {
  // Local typedefs are stripped off the template args.
  typedef int I;
  A<I> a = 1;
  typedef I J;
  A<J> b = a;
  typedef J JA[3];
  A<JA> c = b;
  typedef J* PJ;
  A<PJ> d = c;
  typedef PJ PJA[2][3];
  A<PJA> e = d;
  typedef I(*F)(J,PJ);
  A<F> f = e;
}
template<class T> struct B { T t; };
typedef int I;
typedef I J;
typedef J JB[3];
typedef J* PJ;
typedef PJ PJB[2][3];
typedef I(*F)(J,PJ);
void g() {
  // Typedefs are not stripped off, since they are not local.
  B<I> a = 1;
  B<J> b = a;
  B<JB> c = b;
  B<PJ> d = c;
  B<PJB> e = d;
  B<F> f = e;
}
void h() {
  // Local typedefs are stripped off but nonlocal typedefs are left.
  typedef I LI;
  B<LI> a = 1;
  typedef J LJ;
  B<LJ> b = a;
  typedef JB LJB;
  B<LJB> c = b;
  typedef PJ LPJ;
  B<LPJ> d = c;
  typedef PJB LPJB;
  B<LPJB> e = d;
  typedef F LF;
  B<LF> f = e;
}

