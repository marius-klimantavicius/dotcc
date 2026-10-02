//type: fp
//options: 
# 0 "./analyzer/switch.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/switch.c"


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
# 4 "./analyzer/switch.c" 2

void test (int i)
{
  switch (i)
    {
    case 0:
      __analyzer_eval (i == 0);
      __analyzer_eval (i != -1);
      __analyzer_eval (i != 0);
      __analyzer_eval (i != 1);
      break;

    case 3 ... 5:
      __analyzer_eval (i != 0);
      __analyzer_eval (i > 1);
      __analyzer_eval (i > 2);
      __analyzer_eval (i >= 2);
      __analyzer_eval (i >= 3);
      __analyzer_eval (i <= 5);
      __analyzer_eval (i < 6);
      __analyzer_eval (i <= 6);
      __analyzer_eval (i < 7);
      __analyzer_eval (i != 6);
      __analyzer_eval (i != 3);
      __analyzer_eval (i != 4);
      __analyzer_eval (i != 5);
      __analyzer_eval (i >= 4);
      __analyzer_eval (i >= 5);
      __analyzer_eval (i <= 3);
      __analyzer_eval (i <= 4);
      break;

    default:
      __analyzer_eval (i == -1);
      __analyzer_eval (i == 0);
      __analyzer_eval (i == 2);
      __analyzer_eval (i == 3);
      __analyzer_eval (i == 4);
      __analyzer_eval (i == 5);
      __analyzer_eval (i == 6);
      __analyzer_eval (i != 0);
      __analyzer_eval (i != 1);
      __analyzer_eval (i != 3);
      __analyzer_eval (i != 4);
      __analyzer_eval (i != 5);
      __analyzer_eval (i != 6);
      break;
    }
}





static void __attribute__((noinline))
__analyzer_called_by_test_2 (int y)
{
  switch (y)
    {
    case 0:
      __analyzer_dump_path ();
      break;
    case 1:
      __analyzer_dump_path ();
      break;
    case 2:
      __analyzer_dump_path ();
      break;
    default:
      __analyzer_dump_path ();
      break;
    }
}

void test_2 (int x)
{
  if (x == 1)
    __analyzer_called_by_test_2 (x);
}

void test_3 (int x, int y)
{
  if (y == 3)
    switch (x)
      {
      case 0 ... 9:
      case 20 ... 29:
 if (x == y)
   __analyzer_dump_path ();
 else
   __analyzer_dump_path ();
      }
}

struct s4
{
  unsigned char level:3;
  unsigned char key_id_mode:2;
  unsigned char reserved:3;
};

void test_4 (struct s4 *p)
{
  switch (p->key_id_mode)
    {
    case 0:
      __analyzer_dump_path ();
      break;
    case 1:
      __analyzer_dump_path ();
      break;
    case 2:
      __analyzer_dump_path ();
      break;
    case 3:
      __analyzer_dump_path ();
      break;
    }
  __analyzer_dump_path ();
}

int test_5 (unsigned v)
{
  switch (v)
    {
    case 0:
      return 7;
      break;
    case 1:
      return 23;
      break;
    default:
      return v * 2;
    }
}

int test_6 (unsigned v)
{
  switch (v)
    {
    case 0:
      return 3;
    case -1:
      return 22;
    }
  return -3;
}

int g7 = -1;
int test_7 ()
{
 switch (g7++) {
 case 0:
   return 32;

 case 100:
   return 42;
 }
 return 0;
}

int test_bitmask_1 (int x)
{
  int flag = 0;
  if (x & 0x80)
    flag = 1;

  switch (x)
    {
    case 0:
      if (flag)
 __analyzer_dump_path ();
      else
 __analyzer_dump_path ();
      break;

    case 0x80:
      if (flag)
 __analyzer_dump_path ();
      else
 __analyzer_dump_path ();
      break;

    case 0x81:
      if (flag)
 __analyzer_dump_path ();
      else
 __analyzer_dump_path ();
      break;
    }
}

int test_bitmask_2 (int x)
{
  int flag = 0;
  if ((x & 0xf80) == 0x80)
    flag = 1;

  switch (x)
    {
    case 0:
      if (flag)
 __analyzer_dump_path ();
      else
 __analyzer_dump_path ();
      break;

    case 0x80:
      if (flag)
 __analyzer_dump_path ();
      else
 __analyzer_dump_path ();
      break;

    case 0x81:
      if (flag)
 __analyzer_dump_path ();
      else
 __analyzer_dump_path ();
      break;

    case 0x180:
      if (flag)
 __analyzer_dump_path ();
      else
 __analyzer_dump_path ();
      break;

    case 0xf80:
      if (flag)
 __analyzer_dump_path ();
      else
 __analyzer_dump_path ();
      break;
    }
}
