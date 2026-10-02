//remark:Parenthesized aggr init
//options:--c++20;rp:--c++20 -DNEG;fn

extern "C" int printf(const char*, ...);

struct convertible {
  int i = 0;
  operator int() {
    return ++i;
  }
};

typedef int aggr[3];

void print_aggr(const aggr& a) {
  printf("a[0] = %d, a[1] = %d, a[2] = %d\n", a[0], a[1], a[2]);
}

int main() {
  printf("Direct inits:\n");

  aggr a(12);
  print_aggr(a);
#ifdef NEG
  const auto& a2 = (aggr)11;
  print_aggr(a2);
  const auto& a3 = aggr(10);
  print_aggr(a3);
#endif
  const auto& a4 = static_cast<aggr>(9);
  print_aggr(a4);
#ifdef NEG
  const aggr& a5 = 8;
  print_aggr(a5);
#endif

  printf("\nConversion inits:\n");
  convertible conv;
  aggr b(conv);
  print_aggr(b);
#ifdef NEG
  const auto& b2 = (aggr)conv;
  print_aggr(b2);
  const auto& b3 = aggr(conv);
  print_aggr(b3);
#endif
  const auto& b4 = static_cast<aggr>(conv);
  print_aggr(b4);
#ifdef NEG
  const auto& b5 = conv;
  print_aggr(b5);
#endif
}
