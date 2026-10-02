//type:rp
//options::--gnu_version 70400:--clang_version 80000:--microsoft_version 1924
//options_all:--c++11

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
  int a;
};

bool test(bool check)
{
  check ? (S(1)) : (S(1),S(2),S(3),S(4),S(5));
  return 0;
}

int main(int argc, char** argv) {
  return test(argc != 1);
}
