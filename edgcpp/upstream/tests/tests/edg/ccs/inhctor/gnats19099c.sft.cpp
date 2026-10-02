//type:rp
//options:--c++17
//options_all:-tused

extern "C" int printf(const char *, ...);

struct B {
  template<class T> B(T o, typename T::Q x) {
    printf("Called template ctor: %d\n", x);
    o.print();
  }
};

struct D : B {
  using B::B;
};

class S {
  using Q = int;
  template<class T> friend B::B(T, typename T::Q);
  void print() {
    printf("Calling S::print()\n");
  }
};

D d(S(), 2);

int main() {
}
