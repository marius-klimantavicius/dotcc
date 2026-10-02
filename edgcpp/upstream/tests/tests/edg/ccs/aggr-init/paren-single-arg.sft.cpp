//type:rp
//options::-DNEG;fn
//options_all:--c++20

extern "C" int printf(const char*, ...);

struct aggr {
  int x, y, z;
};

struct convertible {
  int i = 0;
  operator int() {
    return ++i;
  }
};

void print_aggr(aggr& a) {
  printf("x = %d, y = %d, z = %d\n", a.x, a.y, a.z);
  a.x = a.y = a.z = 999;
}

int main() {
  printf("Direct inits:\n");

  aggr a(12);
  print_aggr(a);
  a = (aggr)11;
  print_aggr(a);
  a = aggr(10);
  print_aggr(a);
  a = static_cast<aggr>(9);
  print_aggr(a);
#ifdef NEG
  a = 8;
  print_aggr(a);
#endif

  printf("\nConversion inits:\n");
  convertible conv;
  aggr b(conv);
  print_aggr(b);
  b = (aggr)conv;
  print_aggr(b);
  b = aggr(conv);
  print_aggr(b);
  b = static_cast<aggr>(conv);
  print_aggr(b);
#ifdef NEG
  b = conv;
  print_aggr(b);
#endif
}
