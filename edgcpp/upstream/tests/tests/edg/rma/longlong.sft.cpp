//options_all:-r -x -tused
//options: --strict;cn:;cp

#pragma __printf_args
extern "C" void printf(char*, ...);
void f() {
  long long x = 1ll;
  long long int y = 2ll;
  long long unsigned int z = 3llu;
  printf("%lld %llu %ll %lld\n", x, z, x, y);
}



