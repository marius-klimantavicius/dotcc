//type:fn
//options_all:--c++

struct A { A() { } };

void test()
{
  char s[] = "abc";
  const A a;
  (0 ? s : a); // 'a' should have type "const A"
}
