//options_all:-r -x -tused
//options: --microsoft -n;cn

struct S {
  int a,b,c,d[0];      // Okay with -m --microsoft
} s;
struct S2 {
  int a, (*b)[0];      // Zero not allowed
} s2;
struct S3 {
  int a, b[0], c;      // Incomplete type not allowed
} s3;
struct S4 {
  int a, (b)[0];       // Okay with -m --microsoft
} s4;
struct S5 {
  int a, (*b[0])[2];   // Okay with -m --microsoft
} s5;
int x[0];              // Zero not allowed
int (*y)[0];           // Zero not allowed
int (*z[0]);           // Zero not allowed


