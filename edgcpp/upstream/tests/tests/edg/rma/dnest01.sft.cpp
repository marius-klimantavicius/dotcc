//options_all:-r -x -tused
//options: --strict;cp

class AA {
public:
  static const int j = 0;
};
class A {
public:
  static const int i = 0;
  void f(int=i);
  class B;
  class C {
    void f(int=i);
  };
};
class A::B : private A, private AA {
  void f(int=i, int=j);
  inline void g(int,int);
};
void A::B::g(int=i,int=j) { }


