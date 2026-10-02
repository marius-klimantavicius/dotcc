//type:rp
//options::--gnu_version 70500
//options_all:--c++14

extern "C" int printf(const char*, ...);

struct A {
  int m_i;
};

template <class... Ts> struct X : public virtual Ts... {
  X(const Ts &... ts) : Ts(ts)... {}
};
X<X<A>, A> y{{{2}}, {3}};
X<A> x{{1}};

int main() {
  printf("x.m_i = %d\n", x.m_i);
  printf("y.m_i = %d\n", y.m_i);
  if (x.m_i != 1 || y.m_i != 3) return 1;
  return 0;
}
