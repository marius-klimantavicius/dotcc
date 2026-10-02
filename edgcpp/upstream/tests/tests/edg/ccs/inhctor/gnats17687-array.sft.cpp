//type:rp
//options_all:--c++17

extern "C" int printf(const char*, ...);

struct A {
  A(int x[5]) {
    ai[0] = x[0];
    ai[1] = x[1];
    ai[2] = x[2];
    ai[3] = x[3];
    ai[4] = x[4];
  }
  int ai[5] = {10, 11, 12, 13, 14};
};

struct B : A {
  using A::A;
  int bi[5] = {20, 21, 22, 23, 24};
};

int barr[5] = {0, 1, 2, 3, 4};
B b(barr);
int b2arr[5] = {5, 6, 7, 8, 9};
B b2 = b2arr;

int main() {
  printf("%d %d %d %d %d\n", b.ai[0], b.ai[1], b.ai[2], b.ai[3], b.ai[4]);
  printf("%d %d %d %d %d\n", b.bi[0], b.bi[1], b.bi[2], b.bi[3], b.bi[4]);

  printf("%d %d %d %d %d\n", b2.ai[0], b2.ai[1], b2.ai[2], b2.ai[3], b2.ai[4]);
  printf("%d %d %d %d %d\n", b2.bi[0], b2.bi[1], b2.bi[2], b2.bi[3], b2.bi[4]);

  if (b.ai[0] != 0 ||
      b.ai[1] != 1 ||
      b.ai[2] != 2 ||
      b.ai[3] != 3 ||
      b.ai[4] != 4) {
    return 1;
  }

  if (b.bi[0] != 20 ||
      b.bi[1] != 21 ||
      b.bi[2] != 22 ||
      b.bi[3] != 23 ||
      b.bi[4] != 24) {
    return 2;
  }

  if (b2.ai[0] != 5 ||
      b2.ai[1] != 6 ||
      b2.ai[2] != 7 ||
      b2.ai[3] != 8 ||
      b2.ai[4] != 9) {
    return 3;
  }

  if (b2.bi[0] != 20 ||
      b2.bi[1] != 21 ||
      b2.bi[2] != 22 ||
      b2.bi[3] != 23 ||
      b2.bi[4] != 24) {
    return 4;
  }
}
