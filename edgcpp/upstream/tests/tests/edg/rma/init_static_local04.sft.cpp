//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;rp

extern "C" int printf(const char*...);

class C
{
public:
  int i;
  ~C() { printf("destructing: %d\n", i); }
};

void f(int k)
{
  static C d[] = { {4}, {7}, {10}, {k} };
}

int main()
{
  f(22);
  f(23);
  return 0;
}


