//type: fn
//options: 
# 0 "./spellcheck-stdlib.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./spellcheck-stdlib.c"


void *ptr = NULL;


ptrdiff_t pd;


wchar_t wc;


size_t sz;




void test_stdio_h (void)
{
  FILE *f;


  char buf[BUFSIZ];


  char buf2[FILENAME_MAX];


  stderr;


  stdin;


  stdout;


  EOF;

}



void test_stdlib (int i)
{
  i = EXIT_SUCCESS;

  i = EXIT_FAILURE;

}



int test_errno_h (void)
{
  return errno;

}



void test_stdarg_h (void)
{
  va_list ap;

}


int test_INT_MAX (void)
{
  return INT_MAX;



}


float test_FLT_MAX = FLT_MAX;
