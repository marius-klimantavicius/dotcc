//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;rp

extern "C" int printf(char*, ...);
class Base { };
class Derived : public Base {
public:
  operator Base()
       { printf("in Derived::operator Base()\n"); return *this; }
  operator const Base()
       { printf("in Derived::operator const Base()\n"); return *this; }
  operator Base&()
       { printf("in Derived::operator Base&()\n"); return *this; }
  operator const Base&()
       { printf("in Derived::operator const Base&()\n"); return *this; }
};
int main()
{
  Base b;
  Derived d;

  printf("implicit cast to b:\n");
  b = d;
  printf("explicit cast to b:\n");
  b = (Base)d;
  printf("explicit cast to ref b:\n");
  b = (Base &)d;

  printf("implicit cast to bc1:\n");
  const Base bc1 = d;
  printf("explicit cast to bc2:\n");
  const Base bc2 = (const Base)d;
  printf("explicit cast to ref bc3:\n");
  const Base bc3 = (const Base &)d;
}

