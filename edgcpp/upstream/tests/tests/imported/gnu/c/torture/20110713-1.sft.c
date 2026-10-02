//type: fp
//options: 
//require: TARG_SUPPORTS_X86_64 0
/* { dg-do compile } */
/* { dg-require-effective-target ilp32 } */

void *
test (unsigned long long x, unsigned long long y)
{
    return (void *) (unsigned int) (x / y);
}
