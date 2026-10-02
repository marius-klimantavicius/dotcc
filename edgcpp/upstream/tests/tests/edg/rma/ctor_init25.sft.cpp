//options_all:-r -x -tused
//options: --strict;cn

class A { ~A(); public: A(); };
class B {
  A a[3];
public:
  B() : a() { }
  ~B();
} b;

