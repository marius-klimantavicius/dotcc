//type: fn
//options: 
# 0 "./spellcheck-stdlib.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./spellcheck-stdlib.C"


void *ptr = NULL;


ptrdiff_t pd;


size_t sz;




void test_cstdio (void)
{
  FILE *f;




  char buf[BUFSIZ];


  char buf2[FILENAME_MAX];


  stderr;


  stdin;


  stdout;


  EOF;


  fopen ("test.txt");


  printf ("test\n");


  char tmp[16];
  sprintf (tmp, "test\n");

  snprintf (tmp, 16, "test\n");


  getchar ();

}



int test_cerrno (void)
{
  return errno;

}



void test_cstdarg (void)
{
  va_list ap;

}


int test_INT_MAX (void)
{
  return INT_MAX;



}


float test_FLT_MAX = FLT_MAX;





void test_cstring (char *dest, char *src)
{
  memchr(dest, 'a', 4);

  memcmp(dest, src, 4);

  memcpy(dest, src, 4);

  memmove(dest, src, 4);

  memset(dest, 'a', 4);

  strcat(dest, "test");

  strchr("test", 'e');

  strcmp(dest, "test");

  strcpy(dest, "test");

  strerror(0);

  strlen("test");

  strncat(dest, "test", 3);

  strncmp(dest, "test", 3);

  strncpy(dest, "test", 3);

  strrchr("test", 'e');

  strspn(dest, "test");

  strstr(dest, "test");

}



void test_cassert (int a, int b)
{
  assert (a == b);

}



void test_cstdlib (void *q)
{
  void *ptr = malloc (64);

  free (ptr);

  q = realloc (q, 1024);

  q = calloc (8, 8);


  void callback ();
  atexit (callback);

  int i;
  i = EXIT_SUCCESS;

  i = EXIT_FAILURE;

  exit (i);

  abort ();


  getenv ("foo");

}



void test_ctime (void *q, long s, double d)
{
  clock_t c;

  time_t t;

  tm t2;

  d = difftime (0, 0);

  s = mktime (q);

  s = time (0);

  q = asctime (0);

  q = ctime (0);

  q = gmtime (0);

  q = localtime (0);

  char c[2];
  strftime (c, 2, "", 0);

}




namespace some_ns {}

int not_within_namespace (void)
{
  return some_ns::stdout;

}



class some_class {};

int not_within_class (void)
{
  return some_class::stdout;

}
