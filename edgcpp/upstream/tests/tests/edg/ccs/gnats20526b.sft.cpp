//type:cp
//options:--c99

int one = 1;

struct A {
  int i;
};

int test1(void)
{
  volatile struct A a1;
  volatile struct A a2;
  return (one ? a1 : a2).i;
}
