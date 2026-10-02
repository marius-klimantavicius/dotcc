//type:rp
//options_all:--c++20 -w --no_exceptions
//fixing_pr:22122
extern "C" int printf(const char*, ...);

template<typename T, typename... Args>
T* directNew(Args... args) {
  return new T[]{args...};
}

int main() {
  double da[]{1,2,3};
  double* dp = directNew<double>(1.0, 2.0, 3.0);
  for (int i = 0; i < 3; i++) {
    printf("%g - %g\n", da[i], dp[i]);
  }
  
  double* zp = new double[0]{};
  double* implicit_zp = directNew<double>();
}
