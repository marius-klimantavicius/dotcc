//type: fn
//options: --c++17
# 0 "./lookup/operator-5.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lookup/operator-5.C"




namespace N { struct A { }; }

template <class... Ts> void f (Ts... xs) {
  (xs->*...->*N::A{});
  (N::A{}->*...->*xs);
  (xs / ... / N::A{});
  (N::A{} / ... / xs);
  (xs * ... * N::A{});
  (N::A{} * ... * xs);
  (xs + ... + N::A{});
  (N::A{} + ... + xs);
  (xs - ... - N::A{});
  (N::A{} - ... - xs);
  (xs % ... % N::A{});
  (N::A{} % ... % xs);
  (xs & ... & N::A{});
  (N::A{} & ... & xs);
  (xs | ... | N::A{});
  (N::A{} | ... | xs);
  (xs ^ ... ^ N::A{});
  (N::A{} ^ ... ^ xs);
  (xs << ... << N::A{});
  (N::A{} << ... << xs);
  (xs >> ... >> N::A{});
  (N::A{} >> ... >> xs);
  (xs && ... && N::A{});
  (N::A{} && ... && xs);
  (xs || ... || N::A{});
  (N::A{} || ... || xs);
  (xs , ... , N::A{});
  (N::A{} , ... , xs);

  (xs == ... == N::A{});
  (N::A{} == ... == xs);
  (xs != ... != N::A{});
  (N::A{} != ... != xs);
  (xs < ... < N::A{});
  (N::A{} < ... < xs);
  (xs > ... > N::A{});
  (N::A{} > ... > xs);
  (xs <= ... <= N::A{});
  (N::A{} <= ... <= xs);
  (xs >= ... >= N::A{});
  (N::A{} >= ... >= xs);

  (xs += ... += N::A{});
  (N::A{} += ... += xs);
  (xs -= ... -= N::A{});
  (N::A{} -= ... -= xs);
  (xs *= ... *= N::A{});
  (N::A{} *= ... *= xs);
  (xs /= ... /= N::A{});
  (N::A{} /= ... /= xs);
  (xs %= ... %= N::A{});
  (N::A{} %= ... %= xs);
  (xs |= ... |= N::A{});
  (N::A{} |= ... |= xs);
  (xs ^= ... ^= N::A{});
  (N::A{} ^= ... ^= xs);
  (xs <<= ... <<= N::A{});
  (N::A{} <<= ... <<= xs);
  (xs >>= ... >>= N::A{});
  (N::A{} >>= ... >>= xs);
}

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
# 71 "./lookup/operator-5.C" 2

int main() {
  f(N::A());
}
