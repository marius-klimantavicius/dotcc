//type: fp
//options: 
# 0 "./analyzer/pr95000-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr95000-1.c"
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
# 2 "./analyzer/pr95000-1.c" 2

void test_1 (char* x)
{
  char* y=0;
  switch (*x) {
  case 'a':
    y="foo";
  case 'b':
    if (*x=='a') *y='b';

  }
}

void test_switch_char(char x) {
  switch (x) {
  case 'b':
    __analyzer_eval (x == 'b');

  }
}

void test_switch_int(int x) {
  switch (x) {
  case 97:
    __analyzer_eval (x == 97);
  }
}

void test_if_char(char x) {
  if (x == 'b')
    __analyzer_eval (x == 'b');
}

void test_if_int(int x) {
  if (x == 97)
    __analyzer_eval (x == 97);
}
