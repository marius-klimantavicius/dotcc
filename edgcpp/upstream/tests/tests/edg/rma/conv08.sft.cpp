//options_all:-r -x -tused
//options: --strict;cn

struct A {
  template <class T> operator T*();
};

struct B {
  template <class T> operator T*();
};

struct C : public A, public B {};

int main()
{
  C c;
  int i, *p;

  p = c;                          // Error
  p = c.operator int*();          // Error

}

