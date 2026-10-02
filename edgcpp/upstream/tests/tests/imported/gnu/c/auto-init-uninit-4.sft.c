//type: fp
//options: 
# 0 "./auto-init-uninit-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-4.c"
# 10 "./auto-init-uninit-4.c"
# 1 "./uninit-4.c" 1
# 11 "./uninit-4.c"
extern void abort (void);

struct operation {
    short op;
    char rprio;
    char flags;
    char unsignedp;
    long value;
};

extern struct operation cpp_lex (void);

void
cpp_parse_expr (void)
{
  int rprio;
  struct operation op;

  for (;;)
    {
      op = cpp_lex ();

      switch (op.op)
 {
 case 0:
   break;
 case 1:
   return;
 case 2:
   rprio = 1;
   break;
 default:
   return;
 }

      if (op.op == 0)
 return;

      if (rprio != 1)
 abort();
    }
}
# 11 "./auto-init-uninit-4.c" 2
