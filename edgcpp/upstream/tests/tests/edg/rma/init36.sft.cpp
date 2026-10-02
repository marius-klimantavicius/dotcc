//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;rp

extern "C" int printf(const char *,...);

struct A {
        int i;
        operator int() { return i + 20; }
};
struct B {
        A a1, a2;
        int z;
};
A a;
B b = { 4, a, a };

int main()
{
  printf("4 == %d\n",b.a1.i);
  printf("0 == %d\n",b.a2.i);
  printf("20 == %d\n",b.z);
}

