//type: fp
//options: --c89 --strict_gnu
/* { dg-do compile } */
/* { dg-options "-O1 -std=gnu89" } */

/* Test for .GLOBAL_VAR not being renamed into SSA after alias analysis.
   provided by Dale Johannesen in PR 14266.  */

void foo() { bar (); }
main () { foo (); }
