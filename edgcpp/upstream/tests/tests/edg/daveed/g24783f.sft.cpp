//remark:alias-declaration as init-stmt
//options:--c++23;fp:--c++20;fp:--c++20 --diag_error=3466;fn

  int main() {
    float x[3] = { 1, 2, 3 };
    for (using I = int; I p: x) {}
    if (using I = int; I n = 1);
    switch (using I = int; I n = 3) {
      default:;
    }
  }
