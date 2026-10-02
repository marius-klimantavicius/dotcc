//options_all:-r -x -tused
//options: --strict;cn

/* Most of the following should produce an error in C mode, with or
   without later struct definition. */
static struct A a;        /* Internal linkage, incomplete type. */
struct A a2;
static struct B b;        /* Internal linkage, incomplete type. */
struct B b2;              /* Never completed. */
static struct A aa[];     /* Internal linkage, incomplete type. */
static struct A aa[2];    /* Still incomplete. */
struct A aa2[];
struct A aa2[3];
static struct B bb[];     /* Internal linkage, incomplete type. */
static struct B bb[2];
struct B bb2[];           /* Never completed. */
struct B bb2[3];
struct A { int i; };
static struct A aaa[];    /* Internal linkage, incomplete type. */
struct A aaa2[];          /* Equivalent to "struct A aaa2[] = { 0 };". */

