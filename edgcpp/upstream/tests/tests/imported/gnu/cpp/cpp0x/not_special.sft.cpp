//type: fn
//options: --c++11
# 0 "./cpp0x/not_special.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp0x/not_special.C"
# 9 "./cpp0x/not_special.C"
# 1 "/usr/include/assert.h" 1 3 4
# 36 "/usr/include/assert.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 37 "/usr/include/assert.h" 2 3 4
# 65 "/usr/include/assert.h" 3 4

# 65 "/usr/include/assert.h" 3 4
extern "C" {


extern void __assert_fail (const char *__assertion, const char *__file,
      unsigned int __line, const char *__function)
     throw () __attribute__ ((__noreturn__));


extern void __assert_perror_fail (int __errnum, const char *__file,
      unsigned int __line, const char *__function)
     throw () __attribute__ ((__noreturn__));




extern void __assert (const char *__assertion, const char *__file, int __line)
     throw () __attribute__ ((__noreturn__));


}
# 10 "./cpp0x/not_special.C" 2


# 11 "./cpp0x/not_special.C"
template <bool> struct sa;
template <> struct sa<true> {};

struct one {char x[1];};
struct two {char x[2];};

int copy = 0;
int assign = 0;

struct base
{
    base() {}
    base(const base&) {++copy;}
    base& operator=(const base&) {++assign; return *this;}
};

struct derived
    : base
{
    derived() {}
    derived(derived&&) {}
    derived& operator=(derived&&) {return *this;}
};

int test1()
{
    derived d;
    derived d2(static_cast<derived&&>(d));
    
# 39 "./cpp0x/not_special.C" 3 4
   ((
# 39 "./cpp0x/not_special.C"
   copy == 0
# 39 "./cpp0x/not_special.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 39 "./cpp0x/not_special.C"
   "copy == 0"
# 39 "./cpp0x/not_special.C" 3 4
   , "./cpp0x/not_special.C", 39, __PRETTY_FUNCTION__))
# 39 "./cpp0x/not_special.C"
                    ;
    derived d3(d);
    
# 41 "./cpp0x/not_special.C" 3 4
   ((
# 41 "./cpp0x/not_special.C"
   copy == 1
# 41 "./cpp0x/not_special.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 41 "./cpp0x/not_special.C"
   "copy == 1"
# 41 "./cpp0x/not_special.C" 3 4
   , "./cpp0x/not_special.C", 41, __PRETTY_FUNCTION__))
# 41 "./cpp0x/not_special.C"
                    ;
    d2 = static_cast<derived&&>(d);
    
# 43 "./cpp0x/not_special.C" 3 4
   ((
# 43 "./cpp0x/not_special.C"
   assign == 0
# 43 "./cpp0x/not_special.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 43 "./cpp0x/not_special.C"
   "assign == 0"
# 43 "./cpp0x/not_special.C" 3 4
   , "./cpp0x/not_special.C", 43, __PRETTY_FUNCTION__))
# 43 "./cpp0x/not_special.C"
                      ;
    d3 = d;
    
# 45 "./cpp0x/not_special.C" 3 4
   ((
# 45 "./cpp0x/not_special.C"
   assign == 1
# 45 "./cpp0x/not_special.C" 3 4
   ) ? static_cast<void> (0) : __assert_fail (
# 45 "./cpp0x/not_special.C"
   "assign == 1"
# 45 "./cpp0x/not_special.C" 3 4
   , "./cpp0x/not_special.C", 45, __PRETTY_FUNCTION__))
# 45 "./cpp0x/not_special.C"
                      ;
    return 0;
}

int main()
{
    return test1();
}
