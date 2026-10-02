//type:rp
//options:--c++11:--c++17:--microsoft_version 1924 --c++11:--microsoft_version 1924 --c++17

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
  S s = check ? (S(1)) : (S(1),S(2),S(3),S(4),S(5));
  printf("Value of s.a = %d\n", s.a);
  if (s.a != 5) return 1;
  return 0;
}

int main(int argc, char** argv) {
  return test(argc != 1);
}
