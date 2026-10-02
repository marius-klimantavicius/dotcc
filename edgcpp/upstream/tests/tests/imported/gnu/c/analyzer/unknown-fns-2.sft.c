//type: fp
//options: 
# 0 "./analyzer/unknown-fns-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/unknown-fns-2.c"


# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
} max_align_t;
# 4 "./analyzer/unknown-fns-2.c" 2
# 1 "./analyzer/analyzer-decls.h" 1








# 8 "./analyzer/analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 32 "./analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);
# 5 "./analyzer/unknown-fns-2.c" 2

void unknown_fn (void *);

void test_1 (void)
{
  int i;

  i = 42;
  __analyzer_eval (i == 42);

  unknown_fn (
# 15 "./analyzer/unknown-fns-2.c" 3 4
             ((void *)0)
# 15 "./analyzer/unknown-fns-2.c"
                 );
  __analyzer_eval (i == 42);

  unknown_fn (&i);
  __analyzer_eval (i == 42);

  i = 17;
  __analyzer_eval (i == 17);



  unknown_fn (
# 26 "./analyzer/unknown-fns-2.c" 3 4
             ((void *)0)
# 26 "./analyzer/unknown-fns-2.c"
                 );
  __analyzer_eval (i == 17);
}



void test_1a (void (*fn_ptr) (void *))
{
  int i;

  i = 42;
  __analyzer_eval (i == 42);

  fn_ptr (
# 39 "./analyzer/unknown-fns-2.c" 3 4
         ((void *)0)
# 39 "./analyzer/unknown-fns-2.c"
             );
  __analyzer_eval (i == 42);

  fn_ptr (&i);
  __analyzer_eval (i == 42);

  i = 17;
  __analyzer_eval (i == 17);



  fn_ptr (
# 50 "./analyzer/unknown-fns-2.c" 3 4
         ((void *)0)
# 50 "./analyzer/unknown-fns-2.c"
             );
  __analyzer_eval (i == 17);
}

int *global_for_test_2;

void test_2 (void)
{
  int i;

  i = 42;
  __analyzer_eval (i == 42);

  global_for_test_2 = &i;
  unknown_fn (
# 64 "./analyzer/unknown-fns-2.c" 3 4
             ((void *)0)
# 64 "./analyzer/unknown-fns-2.c"
                 );
  __analyzer_eval (i == 42);

  global_for_test_2 = 
# 67 "./analyzer/unknown-fns-2.c" 3 4
                     ((void *)0)
# 67 "./analyzer/unknown-fns-2.c"
                         ;

  i = 17;
  __analyzer_eval (i == 17);



  unknown_fn (
# 74 "./analyzer/unknown-fns-2.c" 3 4
             ((void *)0)
# 74 "./analyzer/unknown-fns-2.c"
                 );
  __analyzer_eval (i == 17);
}

struct used_by_test_3
{
  int *int_ptr;
};

void test_3 (void)
{
  int i;

  struct used_by_test_3 s;
  s.int_ptr = &i;

  i = 42;
  __analyzer_eval (i == 42);

  unknown_fn (
# 93 "./analyzer/unknown-fns-2.c" 3 4
             ((void *)0)
# 93 "./analyzer/unknown-fns-2.c"
                 );
  __analyzer_eval (i == 42);
  __analyzer_eval (s.int_ptr == &i);


  unknown_fn (&s);
  __analyzer_eval (i == 42);
  __analyzer_eval (s.int_ptr == &i);

  s.int_ptr = 
# 102 "./analyzer/unknown-fns-2.c" 3 4
             ((void *)0)
# 102 "./analyzer/unknown-fns-2.c"
                 ;
  __analyzer_eval (s.int_ptr == 
# 103 "./analyzer/unknown-fns-2.c" 3 4
                               ((void *)0)
# 103 "./analyzer/unknown-fns-2.c"
                                   );

  i = 17;
  __analyzer_eval (i == 17);



  unknown_fn (
# 110 "./analyzer/unknown-fns-2.c" 3 4
             ((void *)0)
# 110 "./analyzer/unknown-fns-2.c"
                 );
  __analyzer_eval (i == 17);
}

struct used_by_test_4
{
  int *int_ptr;
};

void test_4 (struct used_by_test_4 *st4_ptr)
{



  int i = 42;
  __analyzer_eval (i == 42);

  unknown_fn (
# 127 "./analyzer/unknown-fns-2.c" 3 4
             ((void *)0)
# 127 "./analyzer/unknown-fns-2.c"
                 );
  __analyzer_eval (i == 42);



  st4_ptr->int_ptr = &i;
  unknown_fn (
# 133 "./analyzer/unknown-fns-2.c" 3 4
             ((void *)0)
# 133 "./analyzer/unknown-fns-2.c"
                 );
  __analyzer_eval (i == 42);


  i = 17;
  __analyzer_eval (i == 17);
  st4_ptr->int_ptr = 
# 139 "./analyzer/unknown-fns-2.c" 3 4
                    ((void *)0)
# 139 "./analyzer/unknown-fns-2.c"
                        ;
  unknown_fn (
# 140 "./analyzer/unknown-fns-2.c" 3 4
             ((void *)0)
# 140 "./analyzer/unknown-fns-2.c"
                 );
  __analyzer_eval (i == 17);
}

static void __attribute__((noinline))
known_fn (void *ptr)
{

}

void test_5 (void)
{
  int i;

  i = 42;
  __analyzer_eval (i == 42);

  known_fn (&i);
  __analyzer_eval (i == 42);

  i = 17;
  __analyzer_eval (i == 17);


  unknown_fn (
# 164 "./analyzer/unknown-fns-2.c" 3 4
             ((void *)0)
# 164 "./analyzer/unknown-fns-2.c"
                 );
  __analyzer_eval (i == 17);
}

extern int __attribute__ ((__pure__))
unknown_pure_fn (void *);

void test_6 (void)
{
  int i;

  i = 42;
  __analyzer_eval (i == 42);

  unknown_pure_fn (&i);
  __analyzer_eval (i == 42);

  i = 17;
  __analyzer_eval (i == 17);


  unknown_fn (
# 185 "./analyzer/unknown-fns-2.c" 3 4
             ((void *)0)
# 185 "./analyzer/unknown-fns-2.c"
                 );
  __analyzer_eval (i == 17);
}

extern void unknown_const_fn (const void *);

void test_7 (void)
{
  int i;

  i = 42;
  __analyzer_eval (i == 42);



  unknown_const_fn (&i);
  __analyzer_eval (i == 42);

  i = 17;
  __analyzer_eval (i == 17);


  unknown_fn (
# 207 "./analyzer/unknown-fns-2.c" 3 4
             ((void *)0)
# 207 "./analyzer/unknown-fns-2.c"
                 );
  __analyzer_eval (i == 17);
}

struct used_by_test_8
{
  int *int_ptr;
};

void test_8 (void)
{
  int i;

  i = 42;
  __analyzer_eval (i == 42);

  struct used_by_test_8 st8;
  st8.int_ptr = &i;




  unknown_const_fn (&st8);
  __analyzer_eval (i == 42);

  i = 17;
  __analyzer_eval (i == 17);


  unknown_fn (
# 236 "./analyzer/unknown-fns-2.c" 3 4
             ((void *)0)
# 236 "./analyzer/unknown-fns-2.c"
                 );
  __analyzer_eval (i == 17);
}
