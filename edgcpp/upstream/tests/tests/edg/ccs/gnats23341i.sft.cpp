//type:rp
//options_all:-x

extern "C" int printf(const char *, ...);

int nalive = 0;
int seed = 0;

struct A {
  int i;
  A() : i(++seed) {
    printf("A::A(), %d\n", i);
    nalive++;
  }
  ~A() {
    printf("A::~A(), %d\n", i);
    nalive--;
  }
  operator int() {
    printf("A::operator int(), %d\n", i);
    return i;
  }
};

int f() {
  printf("throwing...\n");
  throw 0;
}

struct B {
  A a;
  int i;
  B() : i((A(),f())) {}
};

int main () {
  try {
    B b;
  } catch (...) {
    printf("catch\n");
  }
  if (nalive != 0) {
    printf("fail\n");
    return 1;
  }
  return 0;
}
