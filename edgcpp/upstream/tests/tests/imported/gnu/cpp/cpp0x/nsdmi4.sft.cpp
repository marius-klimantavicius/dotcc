//type: rp
//options: --c++11
// { dg-do run { target c++11 } }

int c;

struct A
{
  A() { }
  A(const A&) { }
};

A f() { ++c; return A(); }

struct B
{
  A a = f();
};

int main()
{
  B b1, b2;
  if (c != 2)
    __builtin_abort();
}
