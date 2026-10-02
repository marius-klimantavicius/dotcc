//type:fp
//options::-DNEG;fn
//options_all:-tused --c++17

extern "C" int printf(const char *, ...);

template<typename>
struct X {
  struct InnerB {
    template<class T> InnerB(T o, typename T::Q x) {
      printf("Called template ctor: %d\n", x);
      o.print();
    }
  };

  struct InnerD : InnerB {
    using InnerB::InnerB;
  };
};

class S {
  using Q = int;
#ifndef NEG
  template<class U> template<class T> friend X<U>::InnerB::InnerB(T, typename T::Q);
#endif
  void print() {
    printf("Calling S::print()\n");
  }
};

X<int>::InnerB b(S(), 2); // Accessible due to friend decl
X<int>::InnerD d(S(), 7); // Accessible due to inheriting ctor friend decl

int main() {
}
