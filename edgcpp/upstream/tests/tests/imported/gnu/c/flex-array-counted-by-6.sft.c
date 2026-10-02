//type: rp
//options: 
# 0 "./flex-array-counted-by-6.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./flex-array-counted-by-6.c"






# 1 "./builtin-object-size-common.h" 1
typedef long unsigned int size_t;



  extern void exit (int);
  extern void *malloc (size_t);
  extern void free (void *);
  extern void *calloc (size_t, size_t);
  extern void *alloca (size_t);
  extern void *memcpy (void *, const void *, size_t);
  extern void *memset (void *, int, size_t);
  extern char *strcpy (char *, const char *);
  extern char *strdup (const char *);
  extern char *strndup (const char *, size_t);




unsigned nfails = 0;
# 8 "./flex-array-counted-by-6.c" 2

typedef unsigned short u16;

struct info {
       u16 data_len;
       char data[] __attribute__((counted_by(data_len)));
};

struct foo {
       int a;
       int b;
};

static __attribute__((__noinline__))
struct info *setup ()
{
 struct info *p;
 size_t bytes = 3 * sizeof(struct foo);

 p = (struct info *)malloc (sizeof (struct info) + bytes);
 p->data_len = bytes;

 return p;
}

static void
__attribute__((__noinline__)) report (struct info *p)
{
 struct foo *bar = (struct foo *)p->data;
 do { size_t v = 16; if (__builtin_dynamic_object_size((char *)(bar + 1), 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size((char *)(bar + 1), 1)", __builtin_dynamic_object_size((char *)(bar + 1), 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size((char *)(bar + 1), 1)", __builtin_dynamic_object_size((char *)(bar + 1), 1), v); do { __builtin_printf ("Failure at line: %d\n", 37); nfails++; } while (0); } } while (0);;
 do { size_t v = 8; if (__builtin_dynamic_object_size((char *)(bar + 2), 1) == v) __builtin_printf ("ok:  %s == %zd\n", "__builtin_dynamic_object_size((char *)(bar + 2), 1)", __builtin_dynamic_object_size((char *)(bar + 2), 1)); else { __builtin_printf ("WAT: %s == %zd (expected %zd)\n", "__builtin_dynamic_object_size((char *)(bar + 2), 1)", __builtin_dynamic_object_size((char *)(bar + 2), 1), v); do { __builtin_printf ("Failure at line: %d\n", 38); nfails++; } while (0); } } while (0);;
}

int main(int argc, char *argv[])
{
 struct info *p = setup();
 report(p);
 return 0;
}
