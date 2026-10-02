//type: fp
//options: --c++11
# 0 "./lookup/operator-6.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lookup/operator-6.C"




template<class T, class = void> struct S {
  static constexpr bool is_primary = true;
};

template<class T> struct S<T, decltype(+T())> { };
template<class T> struct S<T, decltype(-T())> { };
template<class T> struct S<T, decltype(*T())> { };
template<class T> struct S<T, decltype(~T())> { };
template<class T> struct S<T, decltype(&T())> { };
template<class T> struct S<T, decltype(!T())> { };
template<class T> struct S<T, decltype(++T())> { };
template<class T> struct S<T, decltype(--T())> { };
template<class T> struct S<T, decltype(T()++)> { };
template<class T> struct S<T, decltype(T()--)> { };

template<class T> struct S<T, decltype(T()->*T())> { };
template<class T> struct S<T, decltype(T() / T())> { };
template<class T> struct S<T, decltype(T() * T())> { };
template<class T> struct S<T, decltype(T() + T())> { };
template<class T> struct S<T, decltype(T() - T())> { };
template<class T> struct S<T, decltype(T() % T())> { };
template<class T> struct S<T, decltype(T() & T())> { };
template<class T> struct S<T, decltype(T() | T())> { };
template<class T> struct S<T, decltype(T() ^ T())> { };
template<class T> struct S<T, decltype(T() << T())> { };
template<class T> struct S<T, decltype(T() >> T())> { };
template<class T> struct S<T, decltype(T() && T())> { };
template<class T> struct S<T, decltype(T() || T())> { };
template<class T> struct S<T, decltype(T(), T())> { };

template<class T> struct S<T, decltype(T() == T())> { };
template<class T> struct S<T, decltype(T() != T())> { };
template<class T> struct S<T, decltype(T() < T())> { };
template<class T> struct S<T, decltype(T() > T())> { };
template<class T> struct S<T, decltype(T() <= T())> { };
template<class T> struct S<T, decltype(T() >= T())> { };




template<class T> struct S<T, decltype(T() += T())> { };
template<class T> struct S<T, decltype(T() -= T())> { };
template<class T> struct S<T, decltype(T() *= T())> { };
template<class T> struct S<T, decltype(T() /= T())> { };
template<class T> struct S<T, decltype(T() %= T())> { };
template<class T> struct S<T, decltype(T() |= T())> { };
template<class T> struct S<T, decltype(T() ^= T())> { };
template<class T> struct S<T, decltype(T() <<= T())> { };
template<class T> struct S<T, decltype(T() >>= T())> { };

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
# 58 "./lookup/operator-6.C" 2

static_assert(S<N::A>::is_primary, "");
