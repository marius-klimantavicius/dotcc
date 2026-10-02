//options_all:-r -x -tused
//options: --strict;cp

template <class T> class A { };
void f() {
  int A = 0;
  ::A<int> x;
  struct S : ::A<char> { };
}
struct S {
  int A;
  struct B : ::A<float> { };
};

