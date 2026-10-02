//type:rp
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct W
{
  W() = default;
  W(int x) : wi(x), wj(x+2) {}

  int wi = 10;
  int wj;
};

struct X : virtual W
{
  using W::W;
  X() : xi(10), xj(13) {}

  int xi = 5;
  int xj;
};

struct Y : X
{
  using X::X;
};

Y y1var(15);
Y y2var;

int main() {
  printf("y1var::wi = %d, y1var::wj = %d\n", y1var.wi, y1var.wj);
  printf("y1var::xi = %d, y1var::xj = %d\n", y1var.xi, y1var.xj);

  printf("y2var::wi = %d, y2var::wj = %d\n", y2var.wi, y2var.wj);
  printf("y2var::xi = %d, y2var::xj = %d\n", y2var.xi, y2var.xj);

  if (y1var.wi != 15 ||
      y1var.wj != 17 ||
      y1var.xi != 5 ||
      y1var.xj != 0) {
    return 1;
  }

  if (y2var.wi != 10 ||
      y2var.wj == 12 ||
      y2var.xi != 10 ||
      y2var.xj != 13) {
    return 2;
  }
}
