//options_all:-r -x -tused
//options: --strict;cn:;ln

struct X {
  X(int);
  X(const volatile X &);
  X(X&);
};
X x = 0;
X f(int i) { return X(i); }
X a[2] = { 0, 0 };
void g(X);
int main() {
  g(X(1));
}

