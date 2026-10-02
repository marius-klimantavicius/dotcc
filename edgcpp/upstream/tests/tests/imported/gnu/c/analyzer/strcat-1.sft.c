//type: fp
//options: 
# 0 "./analyzer/strcat-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/strcat-1.c"


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
# 4 "./analyzer/strcat-1.c" 2

char *strcat (char *dest, const char *src);


char *
test_passthrough (char *dest, const char *src)
{
  return strcat (dest, src);
}

char *
test_null_dest (const char *src)
{
  return strcat (((void *)0), src);
}

char *
test_null_src (char *dest)
{
  return strcat (dest, ((void *)0));
}

char *
test_uninit_dest (const char *src)
{
  char dest[10];
  return strcat (dest, src);
}

char *
test_uninit_src (char *dest)
{
  const char src[10];
  return strcat (dest, src);
}

char *
test_dest_not_terminated (char *src)
{
  char dest[3] = "foo";
  return strcat (dest, src);

}

char *
test_src_not_terminated (char *dest)
{
  const char src[3] = "foo";
  return strcat (dest, src);

}

char * __attribute__((noinline))
call_strcat (char *dest, const char *src)
{
  return strcat (dest, src);
}

void
test_concrete_valid_static_size (void)
{
  char buf[16];
  char *p1 = __builtin_strcpy (buf, "abc");
  char *p2 = call_strcat (buf, "def");
  __analyzer_eval (p1 == buf);
  __analyzer_eval (p2 == buf);
  __analyzer_eval (buf[0] == 'a');
  __analyzer_eval (buf[1] == 'b');
  __analyzer_eval (buf[2] == 'c');
  __analyzer_eval (buf[3] == 'd');
  __analyzer_eval (buf[4] == 'e');
  __analyzer_eval (buf[5] == 'f');
  __analyzer_eval (buf[6] == '\0');
  __analyzer_eval (__builtin_strlen (buf) == 6);
}

void
test_concrete_valid_static_size_2 (void)
{
  char buf[16];
  char *p1 = __builtin_strcpy (buf, "abc");
  char *p2 = call_strcat (buf, "def");
  char *p3 = call_strcat (buf, "ghi");
  __analyzer_eval (p1 == buf);
  __analyzer_eval (p2 == buf);
  __analyzer_eval (p3 == buf);
  __analyzer_eval (buf[0] == 'a');
  __analyzer_eval (buf[1] == 'b');
  __analyzer_eval (buf[2] == 'c');
  __analyzer_eval (buf[3] == 'd');
  __analyzer_eval (buf[4] == 'e');
  __analyzer_eval (buf[5] == 'f');
  __analyzer_eval (buf[6] == 'g');
  __analyzer_eval (buf[7] == 'h');
  __analyzer_eval (buf[8] == 'i');
  __analyzer_eval (buf[9] == '\0');
  __analyzer_eval (__builtin_strlen (buf) == 9);
  __analyzer_eval (__builtin_strlen (buf + 1) == 8);
  __analyzer_eval (__builtin_strlen (buf + 2) == 7);
  __analyzer_eval (__builtin_strlen (buf + 3) == 6);
  __analyzer_eval (__builtin_strlen (buf + 4) == 5);
  __analyzer_eval (__builtin_strlen (buf + 5) == 4);
  __analyzer_eval (__builtin_strlen (buf + 6) == 3);
  __analyzer_eval (__builtin_strlen (buf + 7) == 2);
  __analyzer_eval (__builtin_strlen (buf + 8) == 1);
  __analyzer_eval (__builtin_strlen (buf + 9) == 0);
}

char * __attribute__((noinline))
call_strcat_invalid (char *dest, const char *src)
{
  return strcat (dest, src);
}

void
test_concrete_invalid_static_size (void)
{
  char buf[3];
  buf[0] = '\0';
  call_strcat_invalid (buf, "abc");
}

void
test_concrete_symbolic (const char *suffix)
{
  char buf[10];
  buf[0] = '\0';
  call_strcat (buf, suffix);
}
