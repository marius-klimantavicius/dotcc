//options_all:-r -x -tused
//options: --strict;cn

struct S { S(); };
struct T { };
struct A {
 const S s;
 A() { }
} a;
struct AA {
  const S s;
  const T t;
  const int i;
  AA() { }
} aa;
struct B {
  const S s;
  B(const B&) { }
} b;
struct BB {
  const S s;
  const int i;
  const T t;
  BB(const BB&) { }
} bb;
struct C {
  const S s;
} c;
struct CC {
  const S s;
  const T t;
  const int i;
} cc;



