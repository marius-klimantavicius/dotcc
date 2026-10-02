//type:fn
//options::--c++20:--gnu_version 80000;cp:--gnu_version 80000 --c++20:--clang_version 80000;cp:--clang_version 80000 --c++20;cp
//fixing_pr:22112
struct S { int a, b, c, d, e; };
struct T { int a, b; };
void foo (S);
void bar (T);

void baz ()
{
  foo ({.a = 5, 6, .c = 2, 3});
}
