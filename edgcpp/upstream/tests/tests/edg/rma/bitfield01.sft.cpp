//options_all:-r -x -tused
//options: --strict;cn:;cp

struct A {
  unsigned char a:2;
  signed char b:2;
  char c:2;
  unsigned short d:2;
  signed short e:2;
  short f:2;
  unsigned int g:2;
  signed int h:2;
  int i:2;
  unsigned long j:2;
  signed long k:2;
  long l:2;
  unsigned long long m:2;
  signed long long n:2;
  long long o:2;
};
struct B {
  unsigned char a:1;
  signed char b:1;
  char c:1;
  unsigned short d:1;
  signed short e:1;
  short f:1;
  unsigned int g:1;
  signed int h:1;
  int i:1;
  unsigned long j:1;
  signed long k:1;
  long l:1;
  unsigned long long m:1;
  signed long long n:1;
  long long o:1;
};

