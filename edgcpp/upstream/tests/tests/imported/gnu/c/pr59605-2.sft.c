//type: rp
//options: 
# 0 "./pr59605-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr59605-2.c"





# 1 "./pr59605.c" 1




extern void abort (void);
# 15 "./pr59605.c"
static union {
  char buf[((sizeof (long long)) + (1024 + 8192) + (sizeof (long long)))];
  long long align_int;
  long double align_fp;
} u;

char A[((sizeof (long long)) + (1024 + 8192) + (sizeof (long long)))];

int
main ()
{
  int off, len, i;
  char *p, *q;

  for (i = 0; i < ((sizeof (long long)) + (1024 + 8192) + (sizeof (long long))); i++)
    A[i] = 'A';

  for (off = 0; off < (sizeof (long long)); off++)
    for (len = 1; len < (1024 + 8192); len++)
      {
 for (i = 0; i < ((sizeof (long long)) + (1024 + 8192) + (sizeof (long long))); i++)
   u.buf[i] = 'a';

 p = __builtin_memcpy (u.buf + off, A, len);
 if (p != u.buf + off)
   abort ();

 q = u.buf;
 for (i = 0; i < off; i++, q++)
   if (*q != 'a')
     abort ();

 for (i = 0; i < len; i++, q++)
   if (*q != 'A')
     abort ();

 for (i = 0; i < (sizeof (long long)); i++, q++)
   if (*q != 'a')
     abort ();
      }

  return 0;
}
# 7 "./pr59605-2.c" 2
