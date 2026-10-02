//options_all:-r -x -tused
//options: --strict;cn:;cn

class X;
template <class T> class A {
public:
  void f() throw(X) { }
  void g() throw(X);
};
A<int> a;
template <class T> void A<T>::g() throw(X) { }
class X { };
A<char> c;
main() {
  a.f();
  a.g();
}

