//options_all:-r -x -tused
//options: --strict;cn

class A {
  union { int i; union { char c; short s; }; };
};
A a;
int main() {
  a.s = 0;
}

