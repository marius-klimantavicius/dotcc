//type: fp
//options_all: -A --c++20 -tused -e 200 --no_wrap -A
//
  struct A {
    int q;
    constexpr A(int q) : q(q) { }
    constexpr A(const A &a) : q(a.q * 2) { }
  };

  int main(void) {
    constexpr A a(42);
    constexpr int aq = a.q;
    struct Q {
     int foo() { return a.q; }
    } q;
    return q.foo();
  }

//cwg: 2083
//title: Incorrect cases of odr-use
//meeting: Kona 02/19
//edg_status: Passes
