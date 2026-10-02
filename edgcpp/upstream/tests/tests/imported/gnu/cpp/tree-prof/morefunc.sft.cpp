//type: s
//options: 
# 0 "./tree-prof/morefunc.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-prof/morefunc.C"


# 1 "./tree-prof/reorder_class1.h" 1
struct A {
  virtual int foo();
};

int A::foo()
{
  return 1;
}
# 4 "./tree-prof/morefunc.C" 2
# 1 "./tree-prof/reorder_class2.h" 1

struct B {
  virtual int foo();
};

int B::foo()
{
  return 2;
}
# 5 "./tree-prof/morefunc.C" 2

int g;
# 19 "./tree-prof/morefunc.C"
static __attribute__((always_inline))
void test1 (A *tc)
{
  int i;
  for (i = 0; i < 10000000; i++)
     g += tc->foo();
   if (g<100) g++;
}

static __attribute__((always_inline))
void test2 (B *tc)
{
  int i;
  for (i = 0; i < 10000000; i++)
     g += tc->foo();
}


__attribute__((noinline)) void test_a(A *ap) { test1 (ap); }
__attribute__((noinline)) void test_b(B *bp) { test2 (bp); }


int main()
{
  A* ap = new A();
  B* bp = new B();

  test_a(ap);
  test_b(bp);





}
