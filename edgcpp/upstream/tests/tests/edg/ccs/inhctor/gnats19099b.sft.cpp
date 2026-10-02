//type:rp
//options:--c++17
//options_all:-tused

extern "C" int printf(const char *, ...);

class S;

struct B {
  B(S&& o, int x);
};

class S {
  friend B::B(S&&, int);
  void print() {
    printf("Calling S::print()\n");
  }
};

B::B(S&& o, int x) {
  printf("Called template ctor: %d\n", x);
  o.print();
}

struct D : B {
  using B::B;
};

D d(S(), 2);

int main() {
}
