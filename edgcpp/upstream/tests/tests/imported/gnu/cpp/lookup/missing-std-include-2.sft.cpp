//type: fn
//options: 
# 0 "./lookup/missing-std-include-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lookup/missing-std-include-2.C"
# 9 "./lookup/missing-std-include-2.C"
# 1 "./lookup/empty.h" 1
# 10 "./lookup/missing-std-include-2.C" 2

namespace std
{
  extern int sprintf (char *dst, const char *format, ...);
};

void test (void)
{
  std::string s ("hello world");


  std::cout << 10;

}



void test_2 (void)
{
  std::string s ("hello again");


  std::cout << 10;

}
