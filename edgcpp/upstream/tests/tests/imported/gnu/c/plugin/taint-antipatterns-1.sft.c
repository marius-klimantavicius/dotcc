//type: fp
//options: 
# 0 "./plugin/taint-antipatterns-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./plugin/taint-antipatterns-1.c"




# 1 "./plugin/test-uaccess.h" 1
# 9 "./plugin/test-uaccess.h"
extern long copy_from_user(void *to, const void *from, long n);
extern long copy_to_user(void *to, const void *from, long n);
# 6 "./plugin/taint-antipatterns-1.c" 2



typedef unsigned char u8;
typedef unsigned short int u16;
typedef unsigned int u32;
typedef signed int s32;
typedef long unsigned int size_t;



typedef unsigned int gfp_t;


void kfree(const void *);
void *kmalloc(size_t size, gfp_t flags)
  __attribute__((malloc (kfree)));



struct cmd_1
{
  u32 idx;
  u32 val;
};

static u32 arr[16];

int taint_array_access(void *src)
{
  struct cmd_1 cmd;
  if (copy_from_user(&cmd, src, sizeof(cmd)))
    return -14;




  arr[cmd.idx] = cmd.val;
  return 0;
}

struct cmd_2
{
  s32 idx;
  u32 val;
};

int taint_signed_array_access(void *src)
{
  struct cmd_2 cmd;
  if (copy_from_user(&cmd, src, sizeof(cmd)))
    return -14;
  if (cmd.idx >= 16)
    return -14;





  arr[cmd.idx] = cmd.val;
  return 0;
}

struct cmd_s32_binop
{
  s32 a;
  s32 b;
  s32 result;
};

int taint_divide_by_zero_direct(void *uptr)
{
  struct cmd_s32_binop cmd;
  if (copy_from_user(&cmd, uptr, sizeof(cmd)))
    return -14;


  cmd.result = cmd.a / cmd.b;

  if (copy_to_user (uptr, &cmd, sizeof(cmd)))
    return -14;
  return 0;
}

int taint_divide_by_zero_compound(void *uptr)
{
  struct cmd_s32_binop cmd;
  if (copy_from_user(&cmd, uptr, sizeof(cmd)))
    return -14;





  cmd.result = cmd.a / (cmd.b + 1);

  if (copy_to_user (uptr, &cmd, sizeof(cmd)))
    return -14;
  return 0;
}

int taint_mod_by_zero_direct(void *uptr)
{
  struct cmd_s32_binop cmd;
  if (copy_from_user(&cmd, uptr, sizeof(cmd)))
    return -14;


  cmd.result = cmd.a % cmd.b;

  if (copy_to_user (uptr, &cmd, sizeof(cmd)))
    return -14;
  return 0;
}

int taint_mod_by_zero_compound(void *uptr)
{
  struct cmd_s32_binop cmd;
  if (copy_from_user(&cmd, uptr, sizeof(cmd)))
    return -14;





  cmd.result = cmd.a % (cmd.b + 1);

  if (copy_to_user (uptr, &cmd, sizeof(cmd)))
    return -14;
  return 0;
}
