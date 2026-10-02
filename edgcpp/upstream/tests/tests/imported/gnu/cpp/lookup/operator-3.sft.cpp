//type: fn
//options:  --c++20
# 0 "./lookup/operator-3.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lookup/operator-3.C"




template <class T> void f (T x) {
  +x;
  -x;
  *x;
  ~x;
  &x;
  !x;
  ++x;
  --x;
  x++;
  x--;

  x->*x;
  x / x;
  x * x;
  x + x;
  x - x;
  x % x;
  x & x;
  x | x;
  x ^ x;
  x << x;
  x >> x;
  x && x;
  x || x;
  x, x;

  x == x;
  x != x;
  x < x;
  x > x;
  x <= x;
  x >= x;

  x <=> x;


  x += x;
  x -= x;
  x *= x;
  x /= x;
  x %= x;
  x |= x;
  x ^= x;
  x <<= x;
  x >>= x;
}

namespace N { struct A { }; }

# 1 "./lookup/operator-3-ops.h" 1
void operator+(N::A);
void operator-(N::A);
void operator*(N::A);
void operator~(N::A);

void operator&(N::A) = delete;



void operator!(N::A);
void operator++(N::A);
void operator--(N::A);
void operator++(N::A, int);
void operator--(N::A, int);

void operator->*(N::A, N::A);
void operator/(N::A, N::A);
void operator*(N::A, N::A);
void operator+(N::A, N::A);
void operator-(N::A, N::A);
void operator%(N::A, N::A);
void operator&(N::A, N::A);
void operator|(N::A, N::A);
void operator^(N::A, N::A);
void operator<<(N::A, N::A);
void operator>>(N::A, N::A);
void operator&&(N::A, N::A);
void operator||(N::A, N::A);

void operator,(N::A, N::A) = delete;




void operator==(N::A, N::A);
void operator!=(N::A, N::A);
void operator<(N::A, N::A);
void operator>(N::A, N::A);
void operator<=(N::A, N::A);
void operator>=(N::A, N::A);

void operator<=>(N::A, N::A);


void operator+=(N::A, N::A);
void operator-=(N::A, N::A);
void operator*=(N::A, N::A);
void operator/=(N::A, N::A);
void operator%=(N::A, N::A);
void operator|=(N::A, N::A);
void operator^=(N::A, N::A);
void operator<<=(N::A, N::A);
void operator>>=(N::A, N::A);
# 56 "./lookup/operator-3.C" 2

int main() {
  f(N::A());
}
