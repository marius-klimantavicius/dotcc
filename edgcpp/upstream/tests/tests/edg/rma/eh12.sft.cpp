//options_all:-r -x -tused
//options: --strict;cn:;rp

// Run with --no_exceptions to get the specified diagnostic output.
void f() throw (int);
void g() throw (int) { }                      // error
class A {
  void f() throw (int);
  void g() throw (int) { }
};
void A::f() throw (int) { }                   // error
template <class T> void f(T) throw (int);
template <class T> void f(T) throw (int) { }
template <class T> class X {
public:
  void f() throw (int);
  void g() throw (int) { }
};
template <class T> void X<T>::f() throw (int) { }
main() {
  f(int(0));                                  // error on instantiation
  X<int> a;
  a.f();                                      // error on instantiation
}


