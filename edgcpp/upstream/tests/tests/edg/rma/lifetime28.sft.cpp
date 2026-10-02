//options_all:-r -x -tused
//options: --strict;cn:;cp

// object lifetime around array construction in constructor
struct C {
  C(int);
  ~C();
  operator int();
};
struct A {
  A(int = C(1));
  ~A();
};
main () {
  A a[10] = {1, 2};
}


