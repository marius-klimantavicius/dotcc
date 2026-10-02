//options_all:-r -x -tused
//options: --strict;rp

extern "C" int printf(char*, ...);
struct A {
  int x;
  A(int i = 11) : x(i) { }
//  operator int(...);
//  A& operator +(...);
//  A& operator -(int, ...);
  void operator ()(int i, ...) { printf("%d\n", x+i); }
};
int main() {
  A a(100), aa;
  a(1,2);
  aa(1,2);
}



