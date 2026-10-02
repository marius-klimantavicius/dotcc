//options_all:-r -x -tused
//options: --strict;cn:;rp

struct st2 { char :2; };
struct st4 { char :2; char :2; };
struct st6 { char v[3]; char : 2; };   /* fits in 4 bytes */
struct st8 { char v[3]; char : 2; char : 2; };
struct st9 { char v[7]; char : 2; };   /* fits in 8 bytes */
struct st11 { char v[7]; char : 2; char : 2; };


extern "C" int printf(const char *, ...);
	st2 s;
	st4 s2;
	st6 s3;
	st8 s4;
	st9 s5;
	st11 s6;
main() {
  printf("%d\n", sizeof(s));
  printf("%d\n", sizeof(s2));
  printf("%d\n", sizeof(s3));
  printf("%d\n", sizeof(s4));
  printf("%d\n", sizeof(s5));
  printf("%d\n", sizeof(s6));
}


