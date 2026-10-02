//remark:alias-declaration as init-stmt
//options:--c++23;fp:--c++20;fp:--c++20 --diag_error=3466;fn

int main() {
  switch (using I = int; I n = 3) {
    default:;
  }
}

