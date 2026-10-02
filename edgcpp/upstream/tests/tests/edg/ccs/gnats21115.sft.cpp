//type:rp
//options:--c++14;fn:--c++17
//options_all:--no_exceptions

extern "C" int printf(const char*, ...);

struct B {
  B(int i = 1) {
    printf("In constructor\n");
    x = i;
  }
  
  B(const B &a, B b = B()) {
    x = b.x*100 + a.x*10 + 2;
    printf("In copy constructor, x = %d\n", x);
  }

  int x;
};

B makeB(int i) {
  return {i};
}

int main() {
  int r2, r3, r4;
  
  B b(3);
  B b2(b);
  r2 = b2.x;
  
  b2.x = 5;
  B b3(b2, makeB(3));
  r3 = b3.x;

  // Same as b3, except extra copy ctor call!
  B b4(b2, b);
  r4 = b4.x;

  if (r2 != 100 + 30 + 2) {
    return 2;
  }
  if (r3 != 300 + 50 + 2) {
    return 3;
  }
  if (r4 != 132*100 + 50 + 2) {
    return 4;
  }
}

