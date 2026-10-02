//type: fp
//options: --c11 --strict_gnu
/* Test typeof and typeof_unqual not keywords with -std=gnu11 -fno-asm.  */
/* { dg-do compile } */
/* { dg-options "-std=gnu11 -fno-asm" } */

int typeof = 1;
long typeof_unqual = 2;
