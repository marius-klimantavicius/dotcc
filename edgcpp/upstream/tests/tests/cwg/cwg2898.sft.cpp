//type:fp
//options:--c++23 -A

struct A
{ };

struct B
{
  B(const A &);
  B(A);
};

struct C
{
  C(A);
};

//int f(B);
int f(C);

const A ca;
int j = f(ca);
