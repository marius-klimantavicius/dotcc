//type: rp
//options: 
//options_all: --gnu_version=80200 -tused -e 200 --no_wrap
/* Test case to check if multiversioned functions are still generated if they are
   marked comdat with inline keyword.  */

/* { dg-do run { target i?86-*-* x86_64-*-* } } */
/* { dg-require-ifunc "" }  */
/* { dg-options "-O2" } */


/* Default version.  */
inline int __attribute__ ((target ("default")))
foo ()
{
  return 0;
}

inline int __attribute__ ((target ("popcnt")))
foo ()
{
  return 0;
}

int main ()
{
  return foo ();
}
