//type:rp
//options:--c++11:--c++17:--c++11 --microsoft_version 1924:--c++17 --microsoft_version 1924
//fixing_pr:22249

extern "C" int printf(const char*, ...);

struct S
{
  const int& nontrivial;
  virtual void func() {}
S(int val) : nontrivial(a) {
  printf("CTOR CALLED, setting %d\n", val);
  a = val;
}
S(const S& s) : nontrivial(a) {
  printf("COPY CTOR CALLED, copying %d\n", s.a);
  a = s.a;
}
S(S&& s) : nontrivial(a) {
  printf("MOVE CTOR CALLED, copying %d\n", s.a);
  a = s.a;
}
  int a;
};

bool test(bool check)
{
  S&& s = check ? (S(1)) : (S(1),S(2),S(3),S(4),S(5));
  S&& sg = S(6);
#if defined(_MSC_VER)
  const S& s2 = (1,S(10));
  const S& s2g = S(11);
#endif
  printf("Value of s.a = %d, sg.a = %d\n", s.a, sg.a);
#if defined(_MSC_VER)
  printf("Value of s2.a = %d, s2g.a = %d\n", s2.a, s2g.a);
  if (s2.a != 10 || s2g.a != 11) return 1;
#endif
  if (s.a != 5 || sg.a != 6) return 1;
  return 0;
}

int main(int argc, char** argv) {
  return test(argc != 1);
}
