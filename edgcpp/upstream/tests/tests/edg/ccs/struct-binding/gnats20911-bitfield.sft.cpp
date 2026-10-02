//type:cp
//options::-DNEG;fn
//options_all:--c++20

struct Foo { int a : 2; int b; };

int main() {
  auto[a, b] = Foo();

#if NEG
  [&] { return a; }();
  [&a = a] { return a; }();
  [&a] { return a; }();
#endif /* NEG */

  [&] { return b; }();
  [&b = b] { return b; }();
  [&b] { return b; }();
}
