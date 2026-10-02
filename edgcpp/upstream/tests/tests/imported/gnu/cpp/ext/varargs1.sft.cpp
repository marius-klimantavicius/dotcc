//type: fp
//options: 
# 0 "./ext/varargs1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/varargs1.C"



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./ext/varargs1.C" 2

# 5 "./ext/varargs1.C"
extern "C" void abort();

void *as[5];
int i;

struct A {
  A() { as[i++] = this; }
  A(const A& a) {
    if (&a != as[i-1])
      abort();
    as[i++] = this;
  }
  ~A() {
    if (this != as[--i])
      abort();
  }
};

void f(int i, ...) {
  va_list ap;
  
# 25 "./ext/varargs1.C" 3 4
 __builtin_va_start(
# 25 "./ext/varargs1.C"
 ap
# 25 "./ext/varargs1.C" 3 4
 ,
# 25 "./ext/varargs1.C"
 i
# 25 "./ext/varargs1.C" 3 4
 )
# 25 "./ext/varargs1.C"
                 ;
  A ar = 
# 26 "./ext/varargs1.C" 3 4
        __builtin_va_arg(
# 26 "./ext/varargs1.C"
        ap
# 26 "./ext/varargs1.C" 3 4
        ,
# 26 "./ext/varargs1.C"
        A
# 26 "./ext/varargs1.C" 3 4
        )
# 26 "./ext/varargs1.C"
                      ;
}

int main()
{
  f(42,A());
  if (i != 0)
    abort();
}
