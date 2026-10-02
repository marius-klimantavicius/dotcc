//type: rp
//options: 
# 0 "./inherit/thunk1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./inherit/thunk1.C"



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 5 "./inherit/thunk1.C" 2


# 6 "./inherit/thunk1.C"
extern "C" void abort ();

struct A {
  virtual void f (int, ...) {}
  int i;
};

struct B : virtual public A {
};

struct C : public B {
  C ();
  virtual void f (int, ...);
};

extern C* cp;

C::C () { cp = this; }

void C::f (int i, ...) {
  if (this != cp)
    abort ();
  va_list ap;
  if (i != 3)
    abort ();
  
# 31 "./inherit/thunk1.C" 3 4
 __builtin_va_start(
# 31 "./inherit/thunk1.C"
 ap
# 31 "./inherit/thunk1.C" 3 4
 ,
# 31 "./inherit/thunk1.C"
 i
# 31 "./inherit/thunk1.C" 3 4
 )
# 31 "./inherit/thunk1.C"
                 ;
  if (
# 32 "./inherit/thunk1.C" 3 4
     __builtin_va_arg(
# 32 "./inherit/thunk1.C"
     ap
# 32 "./inherit/thunk1.C" 3 4
     ,
# 32 "./inherit/thunk1.C"
     int
# 32 "./inherit/thunk1.C" 3 4
     ) 
# 32 "./inherit/thunk1.C"
                      != 7)
    abort ();
  
# 34 "./inherit/thunk1.C" 3 4
 __builtin_va_end(
# 34 "./inherit/thunk1.C"
 ap
# 34 "./inherit/thunk1.C" 3 4
 )
# 34 "./inherit/thunk1.C"
            ;
}

C* cp = new C;

int main ()
{
  cp->f (3, 7);
}
