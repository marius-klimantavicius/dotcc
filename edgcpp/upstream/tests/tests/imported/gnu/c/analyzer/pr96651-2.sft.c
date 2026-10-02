//type: fp
//options: 
# 0 "./analyzer/pr96651-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr96651-2.c"
# 1 "./analyzer/analyzer-decls.h" 1
# 20 "./analyzer/analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 2 "./analyzer/pr96651-2.c" 2

extern void unknown_fn (void *, void *);

static int a;
static int b = 42;
int c;
int d = 17;
struct { int x; int y; char rgb[3]; } s = {5, 10, {0x80, 0x40, 0x20}};
void *e = &d;

extern struct _IO_FILE *stderr;




void test (void)
{
  __analyzer_eval (a == 0);
  __analyzer_eval (b == 42);
  __analyzer_eval (c == 0);
  __analyzer_eval (d == 17);
  __analyzer_eval (s.rgb[2] == 0x20);
  __analyzer_eval (e == &d);
  __analyzer_eval (stderr == 0);
}

static void __attribute__((noinline))
__analyzer_called_from_main (void)
{

  __analyzer_eval (a == 0);
  __analyzer_eval (b == 42);
  __analyzer_eval (c == 0);
  __analyzer_eval (d == 17);
  __analyzer_eval (s.rgb[2] == 0x20);
  __analyzer_eval (e == &d);


  __analyzer_eval (stderr == 0);
}

int main (void)
{

  __analyzer_eval (a == 0);
  __analyzer_eval (b == 42);
  __analyzer_eval (c == 0);
  __analyzer_eval (d == 17);
  __analyzer_eval (s.rgb[2] == 0x20);
  __analyzer_eval (e == &d);


  __analyzer_eval (stderr == 0);

  __analyzer_called_from_main ();

  unknown_fn (&a, &c);


  __analyzer_eval (a == 0);


  __analyzer_eval (b == 42);


  __analyzer_eval (c == 0);
  __analyzer_eval (d == 17);
  __analyzer_eval (s.rgb[2] == 0x20);
  __analyzer_eval (e == &d);
  __analyzer_eval (stderr == 0);
}
