//options_all:-r -x -tused
//options: --strict;cn:;cn

struct A {
        struct B {};
        enum E {e1, e2};
        typedef int     T;
};
B b;
E e;
T t;
main() {
  B b;
  E e;
  T t;
}

