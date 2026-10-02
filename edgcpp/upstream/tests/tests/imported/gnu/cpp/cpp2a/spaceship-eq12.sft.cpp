//type: rp
//options: --c++20
# 0 "./cpp2a/spaceship-eq12.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp2a/spaceship-eq12.C"




# 1 "./cpp2a/spaceship-eq11.C" 1



struct A
{
  unsigned char a : 1;
  unsigned char b : 1;
  constexpr bool operator== (const A &) const = default;
};

struct B
{
  unsigned char a : 8;
  int : 0;
  unsigned char b : 7;
  constexpr bool operator== (const B &) const = default;
};

struct C
{
  unsigned char a : 3;
  unsigned char b : 1;
  constexpr bool operator== (const C &) const = default;
};

void
foo (C &x, int y)
{
  x.b = y;
}

int
main ()
{
  A a{}, b{};
  B c{}, d{};
  C e{}, f{};
  a.b = 1;
  d.b = 1;
  foo (e, 0);
  foo (f, 1);
  return a == b || c == d || e == f;
}
# 6 "./cpp2a/spaceship-eq12.C" 2
