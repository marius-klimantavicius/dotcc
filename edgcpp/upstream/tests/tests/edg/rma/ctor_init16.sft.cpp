//options_all:-r -x -tused
//options: --strict;cp

class A {
public:
  A(int);
  A(const A&);
  ~A();
};
A operator +(const A&, const A&);
A a = A(1) + A(2);

