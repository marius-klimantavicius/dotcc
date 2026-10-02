//options_all:-r -x -tused
//options: --strict;cp

/* C mode */
const void a();
volatile void b();
const volatile void c();

const void a() { }               /* error in strict C mode */
volatile void b() { }            /* error in strict C mode */
const volatile void c() { }      /* error in strict C mode */



