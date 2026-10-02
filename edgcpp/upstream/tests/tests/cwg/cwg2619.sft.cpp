//type:fp
//options_all:--c++23 -A
struct S {
  explicit S(int){}
};
struct A {
  S s;
};
struct B {
  union {
    S s;
  };
};
int main() {
  A a1 = {.s{0}};  // #1
  A a2{.s{0}};     // #2
  B b1 = {.s{0}};  // #3
  B b2{.s{0}};     // #4
}

//cwg: 2619
//title: Kind of initialization for a designated-initializer-list
//meeting: Kona 11/22
//edg_status: EDGcpfe/25853
//fixed_in: 6.7
