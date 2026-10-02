//type: fn
//options: --c++14
# 0 "./lookup/operator-3a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lookup/operator-3a.C"




template <class...> auto f () {
  return [] (auto x) {
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




    x += x;
    x -= x;
    x *= x;
    x /= x;
    x %= x;
    x |= x;
    x ^= x;
    x <<= x;
    x >>= x;
  };
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




void operator+=(N::A, N::A);
void operator-=(N::A, N::A);
void operator*=(N::A, N::A);
void operator/=(N::A, N::A);
void operator%=(N::A, N::A);
void operator|=(N::A, N::A);
void operator^=(N::A, N::A);
void operator<<=(N::A, N::A);
void operator>>=(N::A, N::A);
# 58 "./lookup/operator-3a.C" 2

int main() {
  f()(N::A());
}
