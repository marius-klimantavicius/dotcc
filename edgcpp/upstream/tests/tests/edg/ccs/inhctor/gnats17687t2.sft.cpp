//type:rp
//options_all:--c++17 -tused

extern "C" int printf(const char*, ...);

template <class T>
struct A {
  T n;
  A(double d, T j) : n(1000 * (int)d + j) { }
  A(T i, T j, T k=3, T m=4) : n(i+j+k+m) { }
};

template <class T>
struct B : A<T> {
  using A<T>::A;
};

int main()
{
  B<int> b(2, 3);

  printf("%d\n", b.n);

  if (b.n != 12) return 1;
}
