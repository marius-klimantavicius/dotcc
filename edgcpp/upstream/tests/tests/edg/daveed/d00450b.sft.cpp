//remark:Packed types and back ends
//type:rp
//name:
//options:;rp:-DNEG;fn
//options_all:--g++
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

extern "C" int printf(const char*,...);
int testVar1;

void f();
void f(int);

int a[2];

int main()
{
  int i;
  i = __builtin_constant_p(&testVar1);
  printf("%d\n", i);
  i = __builtin_constant_p(a);
  printf("%d\n", i);
#ifdef NEG
  i = __builtin_constant_p(f);
#else
  i = __builtin_constant_p(3.0);
#endif
  printf("%d\n", i);
  return 0;
}


