//type: fp
//options: 
# 0 "./analyzer/bitfields-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/bitfields-1.c"
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
# 2 "./analyzer/bitfields-1.c" 2

typedef unsigned char u8;
typedef unsigned short int u16;
typedef unsigned int u32;

struct st1
{
  u16 nonzero_offset;
  unsigned int f0 : 1;
  unsigned int f1 : 1;
  unsigned int f2 : 1;
  unsigned int f3 : 1;
  unsigned int f4 : 1;
  unsigned int f5 : 1;
  unsigned int f6 : 1;
  unsigned int f7 : 1;
};

void test_1 (void)
{
  struct st1 s;
  s.f0 = 0;
  __analyzer_eval (s.f0 == 0);
  s.f0 = 1;
  __analyzer_eval (s.f0 == 1);

  s.f1 = 0;
  __analyzer_eval (s.f1 == 0);
  s.f1 = 1;
  __analyzer_eval (s.f1 == 1);



  s.f6 = 0;
  __analyzer_eval (s.f6 == 0);
  s.f6 = 1;
  __analyzer_eval (s.f6 == 1);

  s.f7 = 0;
  __analyzer_eval (s.f7 == 0);
  s.f7 = 1;
  __analyzer_eval (s.f7 == 1);
};

void test_2 (_Bool v0, _Bool v1, _Bool v2, _Bool v3,
      _Bool v4, _Bool v5, _Bool v6, _Bool v7)
{
  struct st1 s;
  s.f0 = v0;
  s.f1 = v1;
  s.f2 = v2;
  s.f3 = v3;
  s.f4 = v4;
  s.f5 = v5;
  s.f6 = v6;
  s.f7 = v7;

  __analyzer_eval (s.f0 == v0);
  __analyzer_eval (s.f1 == v1);
  __analyzer_eval (s.f2 == v2);
  __analyzer_eval (s.f3 == v3);
  __analyzer_eval (s.f4 == v4);
  __analyzer_eval (s.f5 == v5);
  __analyzer_eval (s.f6 == v6);
  __analyzer_eval (s.f7 == v7);
};

struct st3
{
  unsigned int f01 : 2;
  unsigned int f23 : 2;
  unsigned int f34 : 2;
  unsigned int f56 : 2;
};

void test_3 (void)
{
  struct st3 s;
  s.f01 = 0;
  __analyzer_eval (s.f01 == 0);
  s.f01 = 1;
  __analyzer_eval (s.f01 == 1);
  s.f01 = 2;
  __analyzer_eval (s.f01 == 2);
  s.f01 = 3;
  __analyzer_eval (s.f01 == 3);



  s.f56 = 0;
  __analyzer_eval (s.f56 == 0);
  s.f56 = 1;
  __analyzer_eval (s.f56 == 1);
  s.f56 = 2;
  __analyzer_eval (s.f56 == 2);
  s.f56 = 3;
  __analyzer_eval (s.f56 == 3);
};



struct st4
{
  signed int f012 : 3;
  signed int f345 : 3;
};

void test_4 (void)
{
  struct st4 s;
  s.f345 = -4;
  __analyzer_eval (s.f345 == -4);
  s.f345 = -3;
  __analyzer_eval (s.f345 == -3);
  s.f345 = -2;
  __analyzer_eval (s.f345 == -2);
  s.f345 = -1;
  __analyzer_eval (s.f345 == -1);
  s.f345 = 0;
  __analyzer_eval (s.f345 == 0);
  s.f345 = 1;
  __analyzer_eval (s.f345 == 1);
  s.f345 = 2;
  __analyzer_eval (s.f345 == 2);
  s.f345 = 3;
  __analyzer_eval (s.f345 == 3);
};



struct st5
{
  unsigned f0 : 5;
  unsigned :0;
  unsigned f1 : 16;
};

void test_5 (void)
{
  struct st5 s;
  s.f1 = 0xcafe;
  __analyzer_eval (s.f1 == 0xcafe);
}
