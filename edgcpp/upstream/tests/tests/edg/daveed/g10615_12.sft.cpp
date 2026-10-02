//remark:Move and copy operations
//options:--c++11 -A;rp

extern "C" int printf(char const*, ...);

struct M {
  int operator=(M&&) {
    printf("%s\n", "Move-assign M");
    return 0;
  }
};

struct S {
  M x[3];
};

int main() {
  printf("%s\n", "## Test M");
  M m;
  m = M();
  
  printf("%s\n", "## Test S");
  S s;
  s = S();
}

