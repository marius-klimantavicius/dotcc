//type:cp
//options_all:-d-dump_init

struct A {
  A();
};

void f() {
  A a = A();
}
