//type:rp
//options_all:--c++17 -tused

extern "C" int printf(const char*, ...);

struct A {
  int n;

  template <class T>
  A(double d, T j) : n(1000 * (int)d + j) { }
  
  template <class T>
  A(T i, T j, T k=3, T m=4) : n(i+j+k+m) { }
};

struct B : A {
  using A::A;
};

int main()
{
  B b(2, 3);

  printf("%d\n", b.n);

  if (b.n != 12) return 1;
}
