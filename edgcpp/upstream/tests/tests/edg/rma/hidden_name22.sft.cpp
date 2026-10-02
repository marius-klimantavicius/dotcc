//options_all:-r -x -tused
//options: --strict;cn:;cn

extern int f();
main() {
  int f;                     /* Original ::f is not visible */
  {
    extern float f();        /* Is an ext sym created? */
    (void)f();
    {
      int f;
      {
        extern float f();    /* Which is found? */
        (void)f();
      }
    }
  }
}
int f() { return 0; }        /* Diagnostic? */

