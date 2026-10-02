//type: fp
//options: 
# 0 "./lto/20090312_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20090312_0.C"
# 1 "./lto/20090312.h" 1
enum Values { ONE, TWO, THREE };
typedef const char * (* JSErrorCallback)(void *, const char *, const int);
# 2 "./lto/20090312_0.C" 2

extern "C" {
    extern enum Values x;
    extern JSErrorCallback p;
};

int
main()
{
  if ( x == ONE && p == 0)
    return 0;

  return 1;
}
