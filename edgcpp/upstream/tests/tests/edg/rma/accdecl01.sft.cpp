//options_all:-r -x -tused
//options: --strict;cn

class A { int x, y, z; public: int i, j, k; protected: int a, b, c; };
class B { int a, b, c; };
class C : private A {
  public:
    A::i;
    A::x;
    A::a;
    B::a;
    int c;
  protected:
    A::b;
    A::j;
    A::y;
    A::c;
    int y;
    int j;
    int b;
  private:
    A::z;
    A::k;
};

