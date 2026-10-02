//remark:auto(x)/auto{x}
//options:--c++23;fp

struct S { int x; };
S const x = { 42 };
auto r = auto(x);
auto s = auto{x};
int main() {
  r = s;
  s = r;
}
