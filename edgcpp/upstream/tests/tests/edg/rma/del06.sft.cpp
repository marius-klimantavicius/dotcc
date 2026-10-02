//options_all:-r -x -tused
//options: --strict;cn

class B {
  void operator delete(void *);
};
class D : public B;
void f(D *p) {
  delete p;
}

