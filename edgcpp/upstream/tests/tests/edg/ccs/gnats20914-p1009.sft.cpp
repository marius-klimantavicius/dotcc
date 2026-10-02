//type:rp
//options_all:--c++20 -w --no_exceptions

extern "C" int printf(const char*, ...);

int main() {
  double da[]{1,2,3};
  double* dp = new double[]{1,2,3};
  for (int i = 0; i < 3; i++) {
    printf("%g - %g\n", da[i], dp[i]);
  }
  
  double* zp = new double[0]{};
  double* implicit_zp = new double[]{};

  char ca[]{"Hello"};
  char* cp = new char[]{"Hello"};
  printf("%s - %s\n", ca, cp);
}
