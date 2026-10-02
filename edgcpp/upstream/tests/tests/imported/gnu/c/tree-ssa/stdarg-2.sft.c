//type: fp
//options: 
# 0 "./tree-ssa/stdarg-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/stdarg-2.c"







# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 9 "./tree-ssa/stdarg-2.c" 2


# 10 "./tree-ssa/stdarg-2.c"
extern void foo (int, va_list);
extern void bar (int);
long x;
double d;
va_list gap;
va_list *pap;

void
f1 (int i, ...)
{
  va_list ap;
  
# 21 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 21 "./tree-ssa/stdarg-2.c"
 ap, i
# 21 "./tree-ssa/stdarg-2.c" 3 4
 )
# 21 "./tree-ssa/stdarg-2.c"
                 ;
  
# 22 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 22 "./tree-ssa/stdarg-2.c"
 ap
# 22 "./tree-ssa/stdarg-2.c" 3 4
 )
# 22 "./tree-ssa/stdarg-2.c"
            ;
}
# 33 "./tree-ssa/stdarg-2.c"
void
f2 (int i, ...)
{
  va_list ap;
  
# 37 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 37 "./tree-ssa/stdarg-2.c"
 ap, i
# 37 "./tree-ssa/stdarg-2.c" 3 4
 )
# 37 "./tree-ssa/stdarg-2.c"
                 ;
  bar (d);
  x = 
# 39 "./tree-ssa/stdarg-2.c" 3 4
     __builtin_va_arg(
# 39 "./tree-ssa/stdarg-2.c"
     ap
# 39 "./tree-ssa/stdarg-2.c" 3 4
     ,
# 39 "./tree-ssa/stdarg-2.c"
     long
# 39 "./tree-ssa/stdarg-2.c" 3 4
     )
# 39 "./tree-ssa/stdarg-2.c"
                      ;
  bar (x);
  
# 41 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 41 "./tree-ssa/stdarg-2.c"
 ap
# 41 "./tree-ssa/stdarg-2.c" 3 4
 )
# 41 "./tree-ssa/stdarg-2.c"
            ;
}
# 54 "./tree-ssa/stdarg-2.c"
void
f3 (int i, ...)
{
  va_list ap;
  
# 58 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 58 "./tree-ssa/stdarg-2.c"
 ap, i
# 58 "./tree-ssa/stdarg-2.c" 3 4
 )
# 58 "./tree-ssa/stdarg-2.c"
                 ;
  d = 
# 59 "./tree-ssa/stdarg-2.c" 3 4
     __builtin_va_arg(
# 59 "./tree-ssa/stdarg-2.c"
     ap
# 59 "./tree-ssa/stdarg-2.c" 3 4
     ,
# 59 "./tree-ssa/stdarg-2.c"
     double
# 59 "./tree-ssa/stdarg-2.c" 3 4
     )
# 59 "./tree-ssa/stdarg-2.c"
                        ;
  
# 60 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 60 "./tree-ssa/stdarg-2.c"
 ap
# 60 "./tree-ssa/stdarg-2.c" 3 4
 )
# 60 "./tree-ssa/stdarg-2.c"
            ;
}
# 71 "./tree-ssa/stdarg-2.c"
void
f4 (int i, ...)
{
  va_list ap;
  
# 75 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 75 "./tree-ssa/stdarg-2.c"
 ap, i
# 75 "./tree-ssa/stdarg-2.c" 3 4
 )
# 75 "./tree-ssa/stdarg-2.c"
                 ;
  x = 
# 76 "./tree-ssa/stdarg-2.c" 3 4
     __builtin_va_arg(
# 76 "./tree-ssa/stdarg-2.c"
     ap
# 76 "./tree-ssa/stdarg-2.c" 3 4
     ,
# 76 "./tree-ssa/stdarg-2.c"
     double
# 76 "./tree-ssa/stdarg-2.c" 3 4
     )
# 76 "./tree-ssa/stdarg-2.c"
                        ;
  foo (i, ap);
  
# 78 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 78 "./tree-ssa/stdarg-2.c"
 ap
# 78 "./tree-ssa/stdarg-2.c" 3 4
 )
# 78 "./tree-ssa/stdarg-2.c"
            ;
}
# 89 "./tree-ssa/stdarg-2.c"
void
f5 (int i, ...)
{
  va_list ap;
  
# 93 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 93 "./tree-ssa/stdarg-2.c"
 ap, i
# 93 "./tree-ssa/stdarg-2.c" 3 4
 )
# 93 "./tree-ssa/stdarg-2.c"
                 ;
  
# 94 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_copy(
# 94 "./tree-ssa/stdarg-2.c"
 gap
# 94 "./tree-ssa/stdarg-2.c" 3 4
 ,
# 94 "./tree-ssa/stdarg-2.c"
 ap
# 94 "./tree-ssa/stdarg-2.c" 3 4
 )
# 94 "./tree-ssa/stdarg-2.c"
                  ;
  bar (i);
  
# 96 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 96 "./tree-ssa/stdarg-2.c"
 ap
# 96 "./tree-ssa/stdarg-2.c" 3 4
 )
# 96 "./tree-ssa/stdarg-2.c"
            ;
  
# 97 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 97 "./tree-ssa/stdarg-2.c"
 gap
# 97 "./tree-ssa/stdarg-2.c" 3 4
 )
# 97 "./tree-ssa/stdarg-2.c"
             ;
}
# 108 "./tree-ssa/stdarg-2.c"
void
f6 (int i, ...)
{
  va_list ap;
  
# 112 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 112 "./tree-ssa/stdarg-2.c"
 ap, i
# 112 "./tree-ssa/stdarg-2.c" 3 4
 )
# 112 "./tree-ssa/stdarg-2.c"
                 ;
  bar (d);
  
# 114 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_arg(
# 114 "./tree-ssa/stdarg-2.c"
 ap
# 114 "./tree-ssa/stdarg-2.c" 3 4
 ,
# 114 "./tree-ssa/stdarg-2.c"
 long
# 114 "./tree-ssa/stdarg-2.c" 3 4
 )
# 114 "./tree-ssa/stdarg-2.c"
                  ;
  
# 115 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_arg(
# 115 "./tree-ssa/stdarg-2.c"
 ap
# 115 "./tree-ssa/stdarg-2.c" 3 4
 ,
# 115 "./tree-ssa/stdarg-2.c"
 long
# 115 "./tree-ssa/stdarg-2.c" 3 4
 )
# 115 "./tree-ssa/stdarg-2.c"
                  ;
  x = 
# 116 "./tree-ssa/stdarg-2.c" 3 4
     __builtin_va_arg(
# 116 "./tree-ssa/stdarg-2.c"
     ap
# 116 "./tree-ssa/stdarg-2.c" 3 4
     ,
# 116 "./tree-ssa/stdarg-2.c"
     long
# 116 "./tree-ssa/stdarg-2.c" 3 4
     )
# 116 "./tree-ssa/stdarg-2.c"
                      ;
  bar (x);
  
# 118 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 118 "./tree-ssa/stdarg-2.c"
 ap
# 118 "./tree-ssa/stdarg-2.c" 3 4
 )
# 118 "./tree-ssa/stdarg-2.c"
            ;
}
# 129 "./tree-ssa/stdarg-2.c"
void
f7 (int i, ...)
{
  va_list ap;
  
# 133 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 133 "./tree-ssa/stdarg-2.c"
 ap, i
# 133 "./tree-ssa/stdarg-2.c" 3 4
 )
# 133 "./tree-ssa/stdarg-2.c"
                 ;
  pap = &ap;
  bar (6);
  
# 136 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 136 "./tree-ssa/stdarg-2.c"
 ap
# 136 "./tree-ssa/stdarg-2.c" 3 4
 )
# 136 "./tree-ssa/stdarg-2.c"
            ;
}
# 147 "./tree-ssa/stdarg-2.c"
void
f8 (int i, ...)
{
  va_list ap;
  
# 151 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 151 "./tree-ssa/stdarg-2.c"
 ap, i
# 151 "./tree-ssa/stdarg-2.c" 3 4
 )
# 151 "./tree-ssa/stdarg-2.c"
                 ;
  pap = &ap;
  bar (d);
  x = 
# 154 "./tree-ssa/stdarg-2.c" 3 4
     __builtin_va_arg(
# 154 "./tree-ssa/stdarg-2.c"
     ap
# 154 "./tree-ssa/stdarg-2.c" 3 4
     ,
# 154 "./tree-ssa/stdarg-2.c"
     long
# 154 "./tree-ssa/stdarg-2.c" 3 4
     )
# 154 "./tree-ssa/stdarg-2.c"
                      ;
  bar (x);
  
# 156 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 156 "./tree-ssa/stdarg-2.c"
 ap
# 156 "./tree-ssa/stdarg-2.c" 3 4
 )
# 156 "./tree-ssa/stdarg-2.c"
            ;
}
# 167 "./tree-ssa/stdarg-2.c"
void
f9 (int i, ...)
{
  va_list ap;
  
# 171 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 171 "./tree-ssa/stdarg-2.c"
 ap, i
# 171 "./tree-ssa/stdarg-2.c" 3 4
 )
# 171 "./tree-ssa/stdarg-2.c"
                 ;
  __asm __volatile ("" : "=r" (pap) : "0" (&ap));
  bar (6);
  
# 174 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 174 "./tree-ssa/stdarg-2.c"
 ap
# 174 "./tree-ssa/stdarg-2.c" 3 4
 )
# 174 "./tree-ssa/stdarg-2.c"
            ;
}
# 185 "./tree-ssa/stdarg-2.c"
void
f10 (int i, ...)
{
  va_list ap;
  
# 189 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 189 "./tree-ssa/stdarg-2.c"
 ap, i
# 189 "./tree-ssa/stdarg-2.c" 3 4
 )
# 189 "./tree-ssa/stdarg-2.c"
                 ;
  __asm __volatile ("" : "=r" (pap) : "0" (&ap));
  bar (d);
  x = 
# 192 "./tree-ssa/stdarg-2.c" 3 4
     __builtin_va_arg(
# 192 "./tree-ssa/stdarg-2.c"
     ap
# 192 "./tree-ssa/stdarg-2.c" 3 4
     ,
# 192 "./tree-ssa/stdarg-2.c"
     long
# 192 "./tree-ssa/stdarg-2.c" 3 4
     )
# 192 "./tree-ssa/stdarg-2.c"
                      ;
  bar (x);
  
# 194 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 194 "./tree-ssa/stdarg-2.c"
 ap
# 194 "./tree-ssa/stdarg-2.c" 3 4
 )
# 194 "./tree-ssa/stdarg-2.c"
            ;
}
# 205 "./tree-ssa/stdarg-2.c"
void
f11 (int i, ...)
{
  va_list ap;
  
# 209 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 209 "./tree-ssa/stdarg-2.c"
 ap, i
# 209 "./tree-ssa/stdarg-2.c" 3 4
 )
# 209 "./tree-ssa/stdarg-2.c"
                 ;
  bar (d);
  x = 
# 211 "./tree-ssa/stdarg-2.c" 3 4
     __builtin_va_arg(
# 211 "./tree-ssa/stdarg-2.c"
     ap
# 211 "./tree-ssa/stdarg-2.c" 3 4
     ,
# 211 "./tree-ssa/stdarg-2.c"
     long
# 211 "./tree-ssa/stdarg-2.c" 3 4
     )
# 211 "./tree-ssa/stdarg-2.c"
                      ;
  x += 
# 212 "./tree-ssa/stdarg-2.c" 3 4
      __builtin_va_arg(
# 212 "./tree-ssa/stdarg-2.c"
      ap
# 212 "./tree-ssa/stdarg-2.c" 3 4
      ,
# 212 "./tree-ssa/stdarg-2.c"
      long
# 212 "./tree-ssa/stdarg-2.c" 3 4
      )
# 212 "./tree-ssa/stdarg-2.c"
                       ;
  x += 
# 213 "./tree-ssa/stdarg-2.c" 3 4
      __builtin_va_arg(
# 213 "./tree-ssa/stdarg-2.c"
      ap
# 213 "./tree-ssa/stdarg-2.c" 3 4
      ,
# 213 "./tree-ssa/stdarg-2.c"
      long
# 213 "./tree-ssa/stdarg-2.c" 3 4
      )
# 213 "./tree-ssa/stdarg-2.c"
                       ;
  bar (x);
  
# 215 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 215 "./tree-ssa/stdarg-2.c"
 ap
# 215 "./tree-ssa/stdarg-2.c" 3 4
 )
# 215 "./tree-ssa/stdarg-2.c"
            ;
}
# 226 "./tree-ssa/stdarg-2.c"
void
f12 (int i, ...)
{
  va_list ap;
  
# 230 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 230 "./tree-ssa/stdarg-2.c"
 ap, i
# 230 "./tree-ssa/stdarg-2.c" 3 4
 )
# 230 "./tree-ssa/stdarg-2.c"
                 ;
  bar (d);
  
# 232 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_arg(
# 232 "./tree-ssa/stdarg-2.c"
 ap
# 232 "./tree-ssa/stdarg-2.c" 3 4
 ,
# 232 "./tree-ssa/stdarg-2.c"
 double
# 232 "./tree-ssa/stdarg-2.c" 3 4
 )
# 232 "./tree-ssa/stdarg-2.c"
                    ;
  
# 233 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_arg(
# 233 "./tree-ssa/stdarg-2.c"
 ap
# 233 "./tree-ssa/stdarg-2.c" 3 4
 ,
# 233 "./tree-ssa/stdarg-2.c"
 double
# 233 "./tree-ssa/stdarg-2.c" 3 4
 )
# 233 "./tree-ssa/stdarg-2.c"
                    ;
  x = 
# 234 "./tree-ssa/stdarg-2.c" 3 4
     __builtin_va_arg(
# 234 "./tree-ssa/stdarg-2.c"
     ap
# 234 "./tree-ssa/stdarg-2.c" 3 4
     ,
# 234 "./tree-ssa/stdarg-2.c"
     double
# 234 "./tree-ssa/stdarg-2.c" 3 4
     )
# 234 "./tree-ssa/stdarg-2.c"
                        ;
  bar (x);
  
# 236 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 236 "./tree-ssa/stdarg-2.c"
 ap
# 236 "./tree-ssa/stdarg-2.c" 3 4
 )
# 236 "./tree-ssa/stdarg-2.c"
            ;
}
# 247 "./tree-ssa/stdarg-2.c"
void
f13 (int i, ...)
{
  va_list ap;
  
# 251 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 251 "./tree-ssa/stdarg-2.c"
 ap, i
# 251 "./tree-ssa/stdarg-2.c" 3 4
 )
# 251 "./tree-ssa/stdarg-2.c"
                 ;
  bar (d);
  x = 
# 253 "./tree-ssa/stdarg-2.c" 3 4
     __builtin_va_arg(
# 253 "./tree-ssa/stdarg-2.c"
     ap
# 253 "./tree-ssa/stdarg-2.c" 3 4
     ,
# 253 "./tree-ssa/stdarg-2.c"
     double
# 253 "./tree-ssa/stdarg-2.c" 3 4
     )
# 253 "./tree-ssa/stdarg-2.c"
                        ;
  x += 
# 254 "./tree-ssa/stdarg-2.c" 3 4
      __builtin_va_arg(
# 254 "./tree-ssa/stdarg-2.c"
      ap
# 254 "./tree-ssa/stdarg-2.c" 3 4
      ,
# 254 "./tree-ssa/stdarg-2.c"
      double
# 254 "./tree-ssa/stdarg-2.c" 3 4
      )
# 254 "./tree-ssa/stdarg-2.c"
                         ;
  x += 
# 255 "./tree-ssa/stdarg-2.c" 3 4
      __builtin_va_arg(
# 255 "./tree-ssa/stdarg-2.c"
      ap
# 255 "./tree-ssa/stdarg-2.c" 3 4
      ,
# 255 "./tree-ssa/stdarg-2.c"
      double
# 255 "./tree-ssa/stdarg-2.c" 3 4
      )
# 255 "./tree-ssa/stdarg-2.c"
                         ;
  bar (x);
  
# 257 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 257 "./tree-ssa/stdarg-2.c"
 ap
# 257 "./tree-ssa/stdarg-2.c" 3 4
 )
# 257 "./tree-ssa/stdarg-2.c"
            ;
}
# 268 "./tree-ssa/stdarg-2.c"
void
f14 (int i, ...)
{
  va_list ap;
  
# 272 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 272 "./tree-ssa/stdarg-2.c"
 ap, i
# 272 "./tree-ssa/stdarg-2.c" 3 4
 )
# 272 "./tree-ssa/stdarg-2.c"
                 ;
  bar (d);
  x = 
# 274 "./tree-ssa/stdarg-2.c" 3 4
     __builtin_va_arg(
# 274 "./tree-ssa/stdarg-2.c"
     ap
# 274 "./tree-ssa/stdarg-2.c" 3 4
     ,
# 274 "./tree-ssa/stdarg-2.c"
     double
# 274 "./tree-ssa/stdarg-2.c" 3 4
     )
# 274 "./tree-ssa/stdarg-2.c"
                        ;
  x += 
# 275 "./tree-ssa/stdarg-2.c" 3 4
      __builtin_va_arg(
# 275 "./tree-ssa/stdarg-2.c"
      ap
# 275 "./tree-ssa/stdarg-2.c" 3 4
      ,
# 275 "./tree-ssa/stdarg-2.c"
      long
# 275 "./tree-ssa/stdarg-2.c" 3 4
      )
# 275 "./tree-ssa/stdarg-2.c"
                       ;
  x += 
# 276 "./tree-ssa/stdarg-2.c" 3 4
      __builtin_va_arg(
# 276 "./tree-ssa/stdarg-2.c"
      ap
# 276 "./tree-ssa/stdarg-2.c" 3 4
      ,
# 276 "./tree-ssa/stdarg-2.c"
      double
# 276 "./tree-ssa/stdarg-2.c" 3 4
      )
# 276 "./tree-ssa/stdarg-2.c"
                         ;
  bar (x);
  
# 278 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 278 "./tree-ssa/stdarg-2.c"
 ap
# 278 "./tree-ssa/stdarg-2.c" 3 4
 )
# 278 "./tree-ssa/stdarg-2.c"
            ;
}
# 289 "./tree-ssa/stdarg-2.c"
inline void __attribute__((always_inline))
f15_1 (va_list ap)
{
  x = 
# 292 "./tree-ssa/stdarg-2.c" 3 4
     __builtin_va_arg(
# 292 "./tree-ssa/stdarg-2.c"
     ap
# 292 "./tree-ssa/stdarg-2.c" 3 4
     ,
# 292 "./tree-ssa/stdarg-2.c"
     double
# 292 "./tree-ssa/stdarg-2.c" 3 4
     )
# 292 "./tree-ssa/stdarg-2.c"
                        ;
  x += 
# 293 "./tree-ssa/stdarg-2.c" 3 4
      __builtin_va_arg(
# 293 "./tree-ssa/stdarg-2.c"
      ap
# 293 "./tree-ssa/stdarg-2.c" 3 4
      ,
# 293 "./tree-ssa/stdarg-2.c"
      long
# 293 "./tree-ssa/stdarg-2.c" 3 4
      )
# 293 "./tree-ssa/stdarg-2.c"
                       ;
  x += 
# 294 "./tree-ssa/stdarg-2.c" 3 4
      __builtin_va_arg(
# 294 "./tree-ssa/stdarg-2.c"
      ap
# 294 "./tree-ssa/stdarg-2.c" 3 4
      ,
# 294 "./tree-ssa/stdarg-2.c"
      double
# 294 "./tree-ssa/stdarg-2.c" 3 4
      )
# 294 "./tree-ssa/stdarg-2.c"
                         ;
}

void
f15 (int i, ...)
{
  va_list ap;
  
# 301 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_c23_va_start(
# 301 "./tree-ssa/stdarg-2.c"
 ap, i
# 301 "./tree-ssa/stdarg-2.c" 3 4
 )
# 301 "./tree-ssa/stdarg-2.c"
                 ;
  f15_1 (ap);
  
# 303 "./tree-ssa/stdarg-2.c" 3 4
 __builtin_va_end(
# 303 "./tree-ssa/stdarg-2.c"
 ap
# 303 "./tree-ssa/stdarg-2.c" 3 4
 )
# 303 "./tree-ssa/stdarg-2.c"
            ;
}
