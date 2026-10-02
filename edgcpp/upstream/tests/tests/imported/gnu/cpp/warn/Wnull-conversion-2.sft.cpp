//type: fp
//options: 
# 0 "./warn/Wnull-conversion-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/Wnull-conversion-2.C"



# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 425 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
} max_align_t;






  typedef decltype(nullptr) nullptr_t;
# 5 "./warn/Wnull-conversion-2.C" 2


# 6 "./warn/Wnull-conversion-2.C"
class Foo {
 public:
  template <typename T1, typename T2>
  static void Compare(const T1& expected, const T2& actual) { }

  template <typename T1, typename T2>
  static void Compare(const T1& expected, T2* actual) { }

};

template<typename T1>
class Foo2 {
 public:
  Foo2(int x);
  template<typename T2> void Bar(T2 y);
};

template<typename T3> void func(T3 x) { }

typedef Foo2<int> MyFooType;

void func1(long int a) {
  MyFooType *foo2 = new MyFooType(
# 28 "./warn/Wnull-conversion-2.C" 3 4
                                 __null
# 28 "./warn/Wnull-conversion-2.C"
                                     );
  foo2->Bar(a);
  func(
# 30 "./warn/Wnull-conversion-2.C" 3 4
      __null
# 30 "./warn/Wnull-conversion-2.C"
          );
  func<int>(
# 31 "./warn/Wnull-conversion-2.C" 3 4
           __null
# 31 "./warn/Wnull-conversion-2.C"
               );
  func<int *>(
# 32 "./warn/Wnull-conversion-2.C" 3 4
             __null
# 32 "./warn/Wnull-conversion-2.C"
                 );
}

int x = 1;

int
main()
{
  int *p = &x;

  Foo::Compare(0, *p);
  Foo::Compare<long int, int>(
# 43 "./warn/Wnull-conversion-2.C" 3 4
                             __null
# 43 "./warn/Wnull-conversion-2.C"
                                 , p);
  Foo::Compare(
# 44 "./warn/Wnull-conversion-2.C" 3 4
              __null
# 44 "./warn/Wnull-conversion-2.C"
                  , p);
  func1(
# 45 "./warn/Wnull-conversion-2.C" 3 4
       __null
# 45 "./warn/Wnull-conversion-2.C"
           );

  return 0;
}
