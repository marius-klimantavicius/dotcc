//options_all:-r -x -tused
//options: --strict;cp

struct A {
  friend void f(A);
  friend void f(int);
} a;
void f(char);
int main () {
  f(a);
}


