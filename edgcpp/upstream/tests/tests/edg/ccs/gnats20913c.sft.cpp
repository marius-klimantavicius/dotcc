//type:cp
//options::-DNEG;fn
//options_all:--c++20 -A
//fixing_pr:22113
struct A {
  int a;
  int&& r;
};

int f();
int &&rf();

int main() {
  A{1, f()};
  A(1, f());
#ifdef NEG
  A{1.0, 1};
#endif
  A(1.0, 1);
  A(1.0, rf());
}
