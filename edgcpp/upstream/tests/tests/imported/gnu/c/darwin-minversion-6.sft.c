//type: fp
//options: 
/* PR target/63810: Test that tiny numbers are preserved in
   six-character macros.  */
/* Added by Lawrence Velázquez <vq@larryv.me>.  */

/* { dg-options "-mmacosx-version-min=10.10.11" } */
/* { dg-do compile { target *-*-darwin* } } */

int
main ()
{
#if __ENVIRONMENT_MAC_OS_X_VERSION_MIN_REQUIRED__ != 101011
  fail me;
#endif
  return 0;
}
