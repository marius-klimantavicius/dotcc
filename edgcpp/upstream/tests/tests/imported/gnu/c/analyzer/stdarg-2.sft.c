//type: fp
//options: 
# 0 "./analyzer/stdarg-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/stdarg-2.c"


# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 4 "./analyzer/stdarg-2.c" 2
# 1 "./analyzer/analyzer-decls.h" 1
# 20 "./analyzer/analyzer-decls.h"

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
# 5 "./analyzer/stdarg-2.c" 2



static void __attribute__((noinline))
__analyzer_called_by_test_1 (int placeholder, ...)
{
  const char *s;
  int i;
  char c;

  va_list ap;
  
# 16 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 16 "./analyzer/stdarg-2.c"
 ap, placeholder
# 16 "./analyzer/stdarg-2.c" 3 4
 )
# 16 "./analyzer/stdarg-2.c"
                           ;

  s = 
# 18 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 18 "./analyzer/stdarg-2.c"
     ap
# 18 "./analyzer/stdarg-2.c" 3 4
     ,
# 18 "./analyzer/stdarg-2.c"
     char *
# 18 "./analyzer/stdarg-2.c" 3 4
     )
# 18 "./analyzer/stdarg-2.c"
                        ;
  __analyzer_eval (s[0] == 'f');

  i = 
# 21 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 21 "./analyzer/stdarg-2.c"
     ap
# 21 "./analyzer/stdarg-2.c" 3 4
     ,
# 21 "./analyzer/stdarg-2.c"
     int
# 21 "./analyzer/stdarg-2.c" 3 4
     )
# 21 "./analyzer/stdarg-2.c"
                     ;
  __analyzer_eval (i == 1066);

  c = (char)
# 24 "./analyzer/stdarg-2.c" 3 4
           __builtin_va_arg(
# 24 "./analyzer/stdarg-2.c"
           ap
# 24 "./analyzer/stdarg-2.c" 3 4
           ,
# 24 "./analyzer/stdarg-2.c"
           int
# 24 "./analyzer/stdarg-2.c" 3 4
           )
# 24 "./analyzer/stdarg-2.c"
                           ;
  __analyzer_eval (c == '@');

  
# 27 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 27 "./analyzer/stdarg-2.c"
 ap
# 27 "./analyzer/stdarg-2.c" 3 4
 )
# 27 "./analyzer/stdarg-2.c"
            ;
}

void test_1 (void)
{
  __analyzer_called_by_test_1 (42, "foo", 1066, '@');
}



static void __attribute__((noinline))
__analyzer_test_2_inner (va_list ap)
{
  const char *s;
  int i;
  char c;

  s = 
# 44 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 44 "./analyzer/stdarg-2.c"
     ap
# 44 "./analyzer/stdarg-2.c" 3 4
     ,
# 44 "./analyzer/stdarg-2.c"
     char *
# 44 "./analyzer/stdarg-2.c" 3 4
     )
# 44 "./analyzer/stdarg-2.c"
                        ;
  __analyzer_eval (s[0] == 'f');

  i = 
# 47 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 47 "./analyzer/stdarg-2.c"
     ap
# 47 "./analyzer/stdarg-2.c" 3 4
     ,
# 47 "./analyzer/stdarg-2.c"
     int
# 47 "./analyzer/stdarg-2.c" 3 4
     )
# 47 "./analyzer/stdarg-2.c"
                     ;
  __analyzer_eval (i == 1066);

  c = (char)
# 50 "./analyzer/stdarg-2.c" 3 4
           __builtin_va_arg(
# 50 "./analyzer/stdarg-2.c"
           ap
# 50 "./analyzer/stdarg-2.c" 3 4
           ,
# 50 "./analyzer/stdarg-2.c"
           int
# 50 "./analyzer/stdarg-2.c" 3 4
           )
# 50 "./analyzer/stdarg-2.c"
                           ;
  __analyzer_eval (c == '@');
}

static void __attribute__((noinline))
__analyzer_test_2_middle (int placeholder, ...)
{
  va_list ap;
  
# 58 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 58 "./analyzer/stdarg-2.c"
 ap, placeholder
# 58 "./analyzer/stdarg-2.c" 3 4
 )
# 58 "./analyzer/stdarg-2.c"
                           ;
  __analyzer_test_2_inner (ap);
  
# 60 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 60 "./analyzer/stdarg-2.c"
 ap
# 60 "./analyzer/stdarg-2.c" 3 4
 )
# 60 "./analyzer/stdarg-2.c"
            ;
}

void test_2 (void)
{
  __analyzer_test_2_middle (42, "foo", 1066, '@');
}



static void __attribute__((noinline))
__analyzer_called_by_test_not_enough_args (int placeholder, ...)
{
  const char *s;
  int i;

  va_list ap;
  
# 77 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 77 "./analyzer/stdarg-2.c"
 ap, placeholder
# 77 "./analyzer/stdarg-2.c" 3 4
 )
# 77 "./analyzer/stdarg-2.c"
                           ;

  s = 
# 79 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 79 "./analyzer/stdarg-2.c"
     ap
# 79 "./analyzer/stdarg-2.c" 3 4
     ,
# 79 "./analyzer/stdarg-2.c"
     char *
# 79 "./analyzer/stdarg-2.c" 3 4
     )
# 79 "./analyzer/stdarg-2.c"
                        ;
  __analyzer_eval (s[0] == 'f');

  i = 
# 82 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 82 "./analyzer/stdarg-2.c"
     ap
# 82 "./analyzer/stdarg-2.c" 3 4
     ,
# 82 "./analyzer/stdarg-2.c"
     int
# 82 "./analyzer/stdarg-2.c" 3 4
     )
# 82 "./analyzer/stdarg-2.c"
                     ;

  
# 84 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 84 "./analyzer/stdarg-2.c"
 ap
# 84 "./analyzer/stdarg-2.c" 3 4
 )
# 84 "./analyzer/stdarg-2.c"
            ;
}

void test_not_enough_args (void)
{
  __analyzer_called_by_test_not_enough_args (42, "foo");
}



static void __attribute__((noinline))
__analyzer_test_not_enough_args_2_inner (va_list ap)
{
  const char *s;
  int i;

  s = 
# 100 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 100 "./analyzer/stdarg-2.c"
     ap
# 100 "./analyzer/stdarg-2.c" 3 4
     ,
# 100 "./analyzer/stdarg-2.c"
     char *
# 100 "./analyzer/stdarg-2.c" 3 4
     )
# 100 "./analyzer/stdarg-2.c"
                        ;
  __analyzer_eval (s[0] == 'f');

  i = 
# 103 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 103 "./analyzer/stdarg-2.c"
     ap
# 103 "./analyzer/stdarg-2.c" 3 4
     ,
# 103 "./analyzer/stdarg-2.c"
     int
# 103 "./analyzer/stdarg-2.c" 3 4
     )
# 103 "./analyzer/stdarg-2.c"
                     ;
}

static void __attribute__((noinline))
__analyzer_test_not_enough_args_2_middle (int placeholder, ...)
{
  va_list ap;
  
# 110 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 110 "./analyzer/stdarg-2.c"
 ap, placeholder
# 110 "./analyzer/stdarg-2.c" 3 4
 )
# 110 "./analyzer/stdarg-2.c"
                           ;
  __analyzer_test_not_enough_args_2_inner (ap);
  
# 112 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 112 "./analyzer/stdarg-2.c"
 ap
# 112 "./analyzer/stdarg-2.c" 3 4
 )
# 112 "./analyzer/stdarg-2.c"
            ;
}

void test_not_enough_args_2 (void)
{
  __analyzer_test_not_enough_args_2_middle (42, "foo");
}



static void __attribute__((noinline))
__analyzer_called_by_test_excess_args (int placeholder, ...)
{
  const char *s;

  va_list ap;
  
# 128 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 128 "./analyzer/stdarg-2.c"
 ap, placeholder
# 128 "./analyzer/stdarg-2.c" 3 4
 )
# 128 "./analyzer/stdarg-2.c"
                           ;

  s = 
# 130 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 130 "./analyzer/stdarg-2.c"
     ap
# 130 "./analyzer/stdarg-2.c" 3 4
     ,
# 130 "./analyzer/stdarg-2.c"
     char *
# 130 "./analyzer/stdarg-2.c" 3 4
     )
# 130 "./analyzer/stdarg-2.c"
                        ;
  __analyzer_eval (s[0] == 'f');

  
# 133 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 133 "./analyzer/stdarg-2.c"
 ap
# 133 "./analyzer/stdarg-2.c" 3 4
 )
# 133 "./analyzer/stdarg-2.c"
            ;
}

void test_excess_args (void)
{
  __analyzer_called_by_test_excess_args (42, "foo", "bar");
}



void test_missing_va_start (int placeholder, ...)
{
  va_list ap;
  int i = 
# 146 "./analyzer/stdarg-2.c" 3 4
         __builtin_va_arg(
# 146 "./analyzer/stdarg-2.c"
         ap
# 146 "./analyzer/stdarg-2.c" 3 4
         ,
# 146 "./analyzer/stdarg-2.c"
         int
# 146 "./analyzer/stdarg-2.c" 3 4
         )
# 146 "./analyzer/stdarg-2.c"
                         ;
}



void test_missing_va_end (int placeholder, ...)
{
  int i;
  va_list ap;
  
# 155 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 155 "./analyzer/stdarg-2.c"
 ap, placeholder
# 155 "./analyzer/stdarg-2.c" 3 4
 )
# 155 "./analyzer/stdarg-2.c"
                           ;
  i = 
# 156 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 156 "./analyzer/stdarg-2.c"
     ap
# 156 "./analyzer/stdarg-2.c" 3 4
     ,
# 156 "./analyzer/stdarg-2.c"
     int
# 156 "./analyzer/stdarg-2.c" 3 4
     )
# 156 "./analyzer/stdarg-2.c"
                     ;
}




int test_missing_va_end_2 (int placeholder, ...)
{
  int i, j;
  va_list ap;
  
# 166 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 166 "./analyzer/stdarg-2.c"
 ap, placeholder
# 166 "./analyzer/stdarg-2.c" 3 4
 )
# 166 "./analyzer/stdarg-2.c"
                           ;
  i = 
# 167 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 167 "./analyzer/stdarg-2.c"
     ap
# 167 "./analyzer/stdarg-2.c" 3 4
     ,
# 167 "./analyzer/stdarg-2.c"
     int
# 167 "./analyzer/stdarg-2.c" 3 4
     )
# 167 "./analyzer/stdarg-2.c"
                     ;
  if (i == 42)
    {
      
# 170 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_end(
# 170 "./analyzer/stdarg-2.c"
     ap
# 170 "./analyzer/stdarg-2.c" 3 4
     )
# 170 "./analyzer/stdarg-2.c"
                ;
      return -1;
    }
  j = 
# 173 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 173 "./analyzer/stdarg-2.c"
     ap
# 173 "./analyzer/stdarg-2.c" 3 4
     ,
# 173 "./analyzer/stdarg-2.c"
     int
# 173 "./analyzer/stdarg-2.c" 3 4
     )
# 173 "./analyzer/stdarg-2.c"
                     ;
  if (j == 1066)
    return -1;
  
# 176 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 176 "./analyzer/stdarg-2.c"
 ap
# 176 "./analyzer/stdarg-2.c" 3 4
 )
# 176 "./analyzer/stdarg-2.c"
            ;
  return 0;
}



void test_va_arg_after_va_end (int placeholder, ...)
{
  int i;
  va_list ap;
  
# 186 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 186 "./analyzer/stdarg-2.c"
 ap, placeholder
# 186 "./analyzer/stdarg-2.c" 3 4
 )
# 186 "./analyzer/stdarg-2.c"
                           ;
  
# 187 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 187 "./analyzer/stdarg-2.c"
 ap
# 187 "./analyzer/stdarg-2.c" 3 4
 )
# 187 "./analyzer/stdarg-2.c"
            ;
  i = 
# 188 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 188 "./analyzer/stdarg-2.c"
     ap
# 188 "./analyzer/stdarg-2.c" 3 4
     ,
# 188 "./analyzer/stdarg-2.c"
     int
# 188 "./analyzer/stdarg-2.c" 3 4
     )
# 188 "./analyzer/stdarg-2.c"
                     ;
}



static void __attribute__((noinline))
__analyzer_called_by_test_type_mismatch_1 (int placeholder, ...)
{
  int i;

  va_list ap;
  
# 199 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 199 "./analyzer/stdarg-2.c"
 ap, placeholder
# 199 "./analyzer/stdarg-2.c" 3 4
 )
# 199 "./analyzer/stdarg-2.c"
                           ;

  i = 
# 201 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 201 "./analyzer/stdarg-2.c"
     ap
# 201 "./analyzer/stdarg-2.c" 3 4
     ,
# 201 "./analyzer/stdarg-2.c"
     int
# 201 "./analyzer/stdarg-2.c" 3 4
     )
# 201 "./analyzer/stdarg-2.c"
                     ;

  
# 203 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 203 "./analyzer/stdarg-2.c"
 ap
# 203 "./analyzer/stdarg-2.c" 3 4
 )
# 203 "./analyzer/stdarg-2.c"
            ;
}

void test_type_mismatch_1 (void)
{
  __analyzer_called_by_test_type_mismatch_1 (42, "foo");
}



static void __attribute__((noinline))
__analyzer_called_by_test_type_mismatch_2 (int placeholder, ...)
{
  const char *str;

  va_list ap;
  
# 219 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 219 "./analyzer/stdarg-2.c"
 ap, placeholder
# 219 "./analyzer/stdarg-2.c" 3 4
 )
# 219 "./analyzer/stdarg-2.c"
                           ;

  str = 
# 221 "./analyzer/stdarg-2.c" 3 4
       __builtin_va_arg(
# 221 "./analyzer/stdarg-2.c"
       ap
# 221 "./analyzer/stdarg-2.c" 3 4
       ,
# 221 "./analyzer/stdarg-2.c"
       const char *
# 221 "./analyzer/stdarg-2.c" 3 4
       )
# 221 "./analyzer/stdarg-2.c"
                                ;

  
# 223 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 223 "./analyzer/stdarg-2.c"
 ap
# 223 "./analyzer/stdarg-2.c" 3 4
 )
# 223 "./analyzer/stdarg-2.c"
            ;
}

void test_type_mismatch_2 (void)
{
  __analyzer_called_by_test_type_mismatch_2 (42, 1066);
}



static void __attribute__((noinline))
__analyzer_test_type_mismatch_3_inner (va_list ap)
{
  const char *str;

  str = 
# 238 "./analyzer/stdarg-2.c" 3 4
       __builtin_va_arg(
# 238 "./analyzer/stdarg-2.c"
       ap
# 238 "./analyzer/stdarg-2.c" 3 4
       ,
# 238 "./analyzer/stdarg-2.c"
       const char *
# 238 "./analyzer/stdarg-2.c" 3 4
       )
# 238 "./analyzer/stdarg-2.c"
                                ;
}

static void __attribute__((noinline))
__analyzer_test_type_mismatch_3_middle (int placeholder, ...)
{
  va_list ap;
  
# 245 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 245 "./analyzer/stdarg-2.c"
 ap, placeholder
# 245 "./analyzer/stdarg-2.c" 3 4
 )
# 245 "./analyzer/stdarg-2.c"
                           ;

  __analyzer_test_type_mismatch_3_inner (ap);

  
# 249 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 249 "./analyzer/stdarg-2.c"
 ap
# 249 "./analyzer/stdarg-2.c" 3 4
 )
# 249 "./analyzer/stdarg-2.c"
            ;
}

void test_type_mismatch_3 (void)
{
  __analyzer_test_type_mismatch_3_middle (42, 1066);
}



static void __attribute__((noinline))
__analyzer_called_by_test_multiple_traversals (int placeholder, ...)
{
  va_list ap;


  {
    int i, j;

    
# 268 "./analyzer/stdarg-2.c" 3 4
   __builtin_c23_va_start(
# 268 "./analyzer/stdarg-2.c"
   ap, placeholder
# 268 "./analyzer/stdarg-2.c" 3 4
   )
# 268 "./analyzer/stdarg-2.c"
                             ;

    i = 
# 270 "./analyzer/stdarg-2.c" 3 4
       __builtin_va_arg(
# 270 "./analyzer/stdarg-2.c"
       ap
# 270 "./analyzer/stdarg-2.c" 3 4
       ,
# 270 "./analyzer/stdarg-2.c"
       int
# 270 "./analyzer/stdarg-2.c" 3 4
       )
# 270 "./analyzer/stdarg-2.c"
                       ;
    __analyzer_eval (i == 1066);

    j = 
# 273 "./analyzer/stdarg-2.c" 3 4
       __builtin_va_arg(
# 273 "./analyzer/stdarg-2.c"
       ap
# 273 "./analyzer/stdarg-2.c" 3 4
       ,
# 273 "./analyzer/stdarg-2.c"
       int
# 273 "./analyzer/stdarg-2.c" 3 4
       )
# 273 "./analyzer/stdarg-2.c"
                       ;
    __analyzer_eval (j == 42);

    
# 276 "./analyzer/stdarg-2.c" 3 4
   __builtin_va_end(
# 276 "./analyzer/stdarg-2.c"
   ap
# 276 "./analyzer/stdarg-2.c" 3 4
   )
# 276 "./analyzer/stdarg-2.c"
              ;
  }


  {
    int i, j;

    
# 283 "./analyzer/stdarg-2.c" 3 4
   __builtin_c23_va_start(
# 283 "./analyzer/stdarg-2.c"
   ap, placeholder
# 283 "./analyzer/stdarg-2.c" 3 4
   )
# 283 "./analyzer/stdarg-2.c"
                             ;

    i = 
# 285 "./analyzer/stdarg-2.c" 3 4
       __builtin_va_arg(
# 285 "./analyzer/stdarg-2.c"
       ap
# 285 "./analyzer/stdarg-2.c" 3 4
       ,
# 285 "./analyzer/stdarg-2.c"
       int
# 285 "./analyzer/stdarg-2.c" 3 4
       )
# 285 "./analyzer/stdarg-2.c"
                       ;
    __analyzer_eval (i == 1066);

    j = 
# 288 "./analyzer/stdarg-2.c" 3 4
       __builtin_va_arg(
# 288 "./analyzer/stdarg-2.c"
       ap
# 288 "./analyzer/stdarg-2.c" 3 4
       ,
# 288 "./analyzer/stdarg-2.c"
       int
# 288 "./analyzer/stdarg-2.c" 3 4
       )
# 288 "./analyzer/stdarg-2.c"
                       ;
    __analyzer_eval (j == 42);

    
# 291 "./analyzer/stdarg-2.c" 3 4
   __builtin_va_end(
# 291 "./analyzer/stdarg-2.c"
   ap
# 291 "./analyzer/stdarg-2.c" 3 4
   )
# 291 "./analyzer/stdarg-2.c"
              ;
  }
}

void test_multiple_traversals (void)
{
  __analyzer_called_by_test_multiple_traversals (0, 1066, 42);
}



static void __attribute__((noinline))
__analyzer_called_by_test_multiple_traversals_2 (int placeholder, ...)
{
  int i, j;
  va_list args1;
  va_list args2;

  
# 309 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 309 "./analyzer/stdarg-2.c"
 args1, placeholder
# 309 "./analyzer/stdarg-2.c" 3 4
 )
# 309 "./analyzer/stdarg-2.c"
                              ;
  
# 310 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_copy(
# 310 "./analyzer/stdarg-2.c"
 args2
# 310 "./analyzer/stdarg-2.c" 3 4
 ,
# 310 "./analyzer/stdarg-2.c"
 args1
# 310 "./analyzer/stdarg-2.c" 3 4
 )
# 310 "./analyzer/stdarg-2.c"
                       ;


  i = 
# 313 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 313 "./analyzer/stdarg-2.c"
     args1
# 313 "./analyzer/stdarg-2.c" 3 4
     ,
# 313 "./analyzer/stdarg-2.c"
     int
# 313 "./analyzer/stdarg-2.c" 3 4
     )
# 313 "./analyzer/stdarg-2.c"
                        ;
  __analyzer_eval (i == 1066);
  j = 
# 315 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 315 "./analyzer/stdarg-2.c"
     args1
# 315 "./analyzer/stdarg-2.c" 3 4
     ,
# 315 "./analyzer/stdarg-2.c"
     int
# 315 "./analyzer/stdarg-2.c" 3 4
     )
# 315 "./analyzer/stdarg-2.c"
                        ;
  __analyzer_eval (j == 42);
  
# 317 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 317 "./analyzer/stdarg-2.c"
 args1
# 317 "./analyzer/stdarg-2.c" 3 4
 )
# 317 "./analyzer/stdarg-2.c"
               ;


  i = 
# 320 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 320 "./analyzer/stdarg-2.c"
     args2
# 320 "./analyzer/stdarg-2.c" 3 4
     ,
# 320 "./analyzer/stdarg-2.c"
     int
# 320 "./analyzer/stdarg-2.c" 3 4
     )
# 320 "./analyzer/stdarg-2.c"
                        ;
  __analyzer_eval (i == 1066);
  j = 
# 322 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 322 "./analyzer/stdarg-2.c"
     args2
# 322 "./analyzer/stdarg-2.c" 3 4
     ,
# 322 "./analyzer/stdarg-2.c"
     int
# 322 "./analyzer/stdarg-2.c" 3 4
     )
# 322 "./analyzer/stdarg-2.c"
                        ;
  __analyzer_eval (j == 42);
  
# 324 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 324 "./analyzer/stdarg-2.c"
 args2
# 324 "./analyzer/stdarg-2.c" 3 4
 )
# 324 "./analyzer/stdarg-2.c"
               ;
}

void test_multiple_traversals_2 (void)
{
  __analyzer_called_by_test_multiple_traversals_2 (0, 1066, 42);
}



static void __attribute__((noinline))
__analyzer_called_by_test_multiple_traversals_3 (int placeholder, ...)
{
  int i, j;
  va_list args1;
  va_list args2;

  
# 341 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 341 "./analyzer/stdarg-2.c"
 args1, placeholder
# 341 "./analyzer/stdarg-2.c" 3 4
 )
# 341 "./analyzer/stdarg-2.c"
                              ;


  i = 
# 344 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 344 "./analyzer/stdarg-2.c"
     args1
# 344 "./analyzer/stdarg-2.c" 3 4
     ,
# 344 "./analyzer/stdarg-2.c"
     int
# 344 "./analyzer/stdarg-2.c" 3 4
     )
# 344 "./analyzer/stdarg-2.c"
                        ;
  __analyzer_eval (i == 1066);


  
# 348 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_copy(
# 348 "./analyzer/stdarg-2.c"
 args2
# 348 "./analyzer/stdarg-2.c" 3 4
 ,
# 348 "./analyzer/stdarg-2.c"
 args1
# 348 "./analyzer/stdarg-2.c" 3 4
 )
# 348 "./analyzer/stdarg-2.c"
                       ;

  j = 
# 350 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 350 "./analyzer/stdarg-2.c"
     args1
# 350 "./analyzer/stdarg-2.c" 3 4
     ,
# 350 "./analyzer/stdarg-2.c"
     int
# 350 "./analyzer/stdarg-2.c" 3 4
     )
# 350 "./analyzer/stdarg-2.c"
                        ;
  __analyzer_eval (j == 42);
  
# 352 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 352 "./analyzer/stdarg-2.c"
 args1
# 352 "./analyzer/stdarg-2.c" 3 4
 )
# 352 "./analyzer/stdarg-2.c"
               ;


  j = 
# 355 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 355 "./analyzer/stdarg-2.c"
     args2
# 355 "./analyzer/stdarg-2.c" 3 4
     ,
# 355 "./analyzer/stdarg-2.c"
     int
# 355 "./analyzer/stdarg-2.c" 3 4
     )
# 355 "./analyzer/stdarg-2.c"
                        ;
  __analyzer_eval (j == 42);
  
# 357 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 357 "./analyzer/stdarg-2.c"
 args2
# 357 "./analyzer/stdarg-2.c" 3 4
 )
# 357 "./analyzer/stdarg-2.c"
               ;
}

void test_multiple_traversals_3 (void)
{
  __analyzer_called_by_test_multiple_traversals_3 (0, 1066, 42);
}



void test_va_copy_after_va_end (int placeholder, ...)
{
  va_list ap1, ap2;
  
# 370 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 370 "./analyzer/stdarg-2.c"
 ap1, placeholder
# 370 "./analyzer/stdarg-2.c" 3 4
 )
# 370 "./analyzer/stdarg-2.c"
                            ;
  
# 371 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 371 "./analyzer/stdarg-2.c"
 ap1
# 371 "./analyzer/stdarg-2.c" 3 4
 )
# 371 "./analyzer/stdarg-2.c"
             ;
  
# 372 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_copy(
# 372 "./analyzer/stdarg-2.c"
 ap2
# 372 "./analyzer/stdarg-2.c" 3 4
 ,
# 372 "./analyzer/stdarg-2.c"
 ap1
# 372 "./analyzer/stdarg-2.c" 3 4
 )
# 372 "./analyzer/stdarg-2.c"
                   ;
  
# 373 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 373 "./analyzer/stdarg-2.c"
 ap2
# 373 "./analyzer/stdarg-2.c" 3 4
 )
# 373 "./analyzer/stdarg-2.c"
             ;
}



void test_leak_of_va_copy (int placeholder, ...)
{
  va_list ap1, ap2;
  
# 381 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 381 "./analyzer/stdarg-2.c"
 ap1, placeholder
# 381 "./analyzer/stdarg-2.c" 3 4
 )
# 381 "./analyzer/stdarg-2.c"
                            ;
  
# 382 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_copy(
# 382 "./analyzer/stdarg-2.c"
 ap2
# 382 "./analyzer/stdarg-2.c" 3 4
 ,
# 382 "./analyzer/stdarg-2.c"
 ap1
# 382 "./analyzer/stdarg-2.c" 3 4
 )
# 382 "./analyzer/stdarg-2.c"
                   ;
  
# 383 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 383 "./analyzer/stdarg-2.c"
 ap1
# 383 "./analyzer/stdarg-2.c" 3 4
 )
# 383 "./analyzer/stdarg-2.c"
             ;
}




void test_double_va_end (int placeholder, ...)
{
  va_list ap;
  
# 392 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 392 "./analyzer/stdarg-2.c"
 ap, placeholder
# 392 "./analyzer/stdarg-2.c" 3 4
 )
# 392 "./analyzer/stdarg-2.c"
                           ;
  
# 393 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 393 "./analyzer/stdarg-2.c"
 ap
# 393 "./analyzer/stdarg-2.c" 3 4
 )
# 393 "./analyzer/stdarg-2.c"
            ;
  
# 394 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 394 "./analyzer/stdarg-2.c"
 ap
# 394 "./analyzer/stdarg-2.c" 3 4
 )
# 394 "./analyzer/stdarg-2.c"
            ;
}



void test_double_va_start (int placeholder, ...)
{
  int i;
  va_list ap;
  
# 403 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 403 "./analyzer/stdarg-2.c"
 ap, placeholder
# 403 "./analyzer/stdarg-2.c" 3 4
 )
# 403 "./analyzer/stdarg-2.c"
                           ;
  
# 404 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 404 "./analyzer/stdarg-2.c"
 ap, placeholder
# 404 "./analyzer/stdarg-2.c" 3 4
 )
# 404 "./analyzer/stdarg-2.c"
                           ;

  
# 406 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 406 "./analyzer/stdarg-2.c"
 ap
# 406 "./analyzer/stdarg-2.c" 3 4
 )
# 406 "./analyzer/stdarg-2.c"
            ;
}



void test_va_copy_before_va_start (int placeholder, ...)
{
  va_list ap1;
  va_list ap2;
  
# 415 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_copy(
# 415 "./analyzer/stdarg-2.c"
 ap2
# 415 "./analyzer/stdarg-2.c" 3 4
 ,
# 415 "./analyzer/stdarg-2.c"
 ap1
# 415 "./analyzer/stdarg-2.c" 3 4
 )
# 415 "./analyzer/stdarg-2.c"
                   ;
  
# 416 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 416 "./analyzer/stdarg-2.c"
 ap2
# 416 "./analyzer/stdarg-2.c" 3 4
 )
# 416 "./analyzer/stdarg-2.c"
             ;
}




va_list global_ap;

static void __attribute__((noinline))
__analyzer_called_by_test_va_arg_after_return (int placeholder, ...)
{
  
# 427 "./analyzer/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 427 "./analyzer/stdarg-2.c"
 global_ap, placeholder
# 427 "./analyzer/stdarg-2.c" 3 4
 )
# 427 "./analyzer/stdarg-2.c"
                                  ;
  
# 428 "./analyzer/stdarg-2.c" 3 4
 __builtin_va_end(
# 428 "./analyzer/stdarg-2.c"
 global_ap
# 428 "./analyzer/stdarg-2.c" 3 4
 )
# 428 "./analyzer/stdarg-2.c"
                   ;
}

void test_va_arg_after_return (void)
{
  int i;
  __analyzer_called_by_test_va_arg_after_return (42, 1066);
  i = 
# 435 "./analyzer/stdarg-2.c" 3 4
     __builtin_va_arg(
# 435 "./analyzer/stdarg-2.c"
     global_ap
# 435 "./analyzer/stdarg-2.c" 3 4
     ,
# 435 "./analyzer/stdarg-2.c"
     int
# 435 "./analyzer/stdarg-2.c" 3 4
     )
# 435 "./analyzer/stdarg-2.c"
                            ;
}
