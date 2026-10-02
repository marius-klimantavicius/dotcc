//type:rp
//options_all:--c++17

extern "C" int printf(const char *, ...);

struct X {
  int xi = 10;
};

struct Y : X {
  using X::X;
  Y(void*); // Suppresses generation of Y::Y()
  int yi = 20;
};

Y y; // Calls X::X()

int main() {
  printf("%d %d\n", y.xi, y.yi);

  if (y.xi != 10 ||
      y.yi != 20) {
    return 1;
  }
}
