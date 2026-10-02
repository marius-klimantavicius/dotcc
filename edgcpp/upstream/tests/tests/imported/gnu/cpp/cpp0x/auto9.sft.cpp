//type: fn
//options: --c++11
# 0 "./cpp0x/auto9.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp0x/auto9.C"




# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 1 3
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wvariadic-macros"

#pragma GCC diagnostic ignored "-Wc++11-extensions"
#pragma GCC diagnostic ignored "-Wc++23-extensions"
# 328 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  typedef long unsigned int size_t;
  typedef long int ptrdiff_t;


  typedef decltype(nullptr) nullptr_t;


#pragma GCC visibility push(default)


  extern "C++" __attribute__ ((__noreturn__, __always_inline__))
  inline void __terminate() noexcept
  {
    void terminate() noexcept __attribute__ ((__noreturn__,__cold__));
    terminate();
  }
#pragma GCC visibility pop
}
# 361 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
namespace __gnu_cxx
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
# 565 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
#pragma GCC visibility push(default)




  __attribute__((__always_inline__))
  constexpr inline bool
  __is_constant_evaluated() noexcept
  {





    return __builtin_is_constant_evaluated();



  }
#pragma GCC visibility pop
}
# 609 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
#pragma GCC visibility push(default)

  extern "C++" __attribute__ ((__noreturn__)) __attribute__((__cold__))
  void
  __glibcxx_assert_fail
    (const char* __file, int __line, const char* __function,
     const char* __condition)
  noexcept;
#pragma GCC visibility pop
}
# 719 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/os_defines.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/os_defines.h" 3
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
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/os_defines.h" 2 3
# 720 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/cpu_defines.h" 1 3
# 723 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3
# 879 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace __gnu_cxx
{
  typedef __decltype(0.0bf16) __bfloat16_t;
}
# 945 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
#pragma GCC diagnostic pop
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 2 3

extern "C++" {

namespace std __attribute__ ((__visibility__ ("default")))
{
# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 3
  class exception
  {
  public:
    exception() noexcept { }
    virtual ~exception() noexcept;

    exception(const exception&) = default;
    exception& operator=(const exception&) = default;
    exception(exception&&) = default;
    exception& operator=(exception&&) = default;




    virtual const char*
    what() const noexcept;
  };



}

}
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/hash_bytes.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/hash_bytes.h" 3
namespace std
{







  size_t
  _Hash_bytes(const void* __ptr, size_t __len, size_t __seed);





  size_t
  _Fnv_hash_bytes(const void* __ptr, size_t __len, size_t __seed);


}
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 2 3



# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 2 3

#pragma GCC visibility push(default)

extern "C++" {

namespace __cxxabiv1
{
  class __class_type_info;
}
# 85 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 3
namespace std
{






  class type_info
  {
  public:




    virtual ~type_info();



    const char* name() const noexcept
    { return __name[0] == '*' ? __name + 1 : __name; }



    bool before(const type_info& __arg) const noexcept;

   
    bool operator==(const type_info& __arg) const noexcept;


    bool operator!=(const type_info& __arg) const noexcept
    { return !operator==(__arg); }



    size_t hash_code() const noexcept
    {

      return _Hash_bytes(name(), __builtin_strlen(name()),
    static_cast<size_t>(0xc70f6907UL));



    }



    virtual bool __is_pointer_p() const;


    virtual bool __is_function_p() const;







    virtual bool __do_catch(const type_info *__thr_type, void **__thr_obj,
       unsigned __outer) const;


    virtual bool __do_upcast(const __cxxabiv1::__class_type_info *__target,
        void **__obj_ptr) const;

  protected:
    const char *__name;

    explicit type_info(const char *__n): __name(__n) { }

  private:


    type_info& operator=(const type_info&) = delete;
    type_info(const type_info&) = delete;
# 168 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 3
  };


  inline bool
  type_info::before(const type_info& __arg) const noexcept
  {




    if (__name[0] != '*' || __arg.__name[0] != '*')
      return __builtin_strcmp (__name, __arg.__name) < 0;
# 188 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 3
    return __name < __arg.__name;
  }






  inline bool
  type_info::operator==(const type_info& __arg) const noexcept
  {
    if (std::__is_constant_evaluated())
      return this == &__arg;

    if (__name == __arg.__name)
      return true;






    return __name[0] != '*' && __builtin_strcmp (__name, __arg.name()) == 0;



  }
# 224 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 3
  class bad_cast : public exception
  {
  public:
    bad_cast() noexcept { }



    virtual ~bad_cast() noexcept;


    virtual const char* what() const noexcept;
  };





  class bad_typeid : public exception
  {
  public:
    bad_typeid () noexcept { }



    virtual ~bad_typeid() noexcept;


    virtual const char* what() const noexcept;
  };
}

}

#pragma GCC visibility pop
# 6 "./cpp0x/auto9.C" 2
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 7 "./cpp0x/auto9.C" 2
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
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
# 8 "./cpp0x/auto9.C" 2


# 9 "./cpp0x/auto9.C"
int i = *(auto *) 0;
struct A *p = (auto *) 0;
int *q = static_cast <auto *>(0);
const int *r = const_cast <auto *>(q);
const std::type_info &t1 = typeid (auto);
const std::type_info &t2 = typeid (auto *);

struct A
{
  operator auto ();
  operator auto *();
};

struct A2
{
  operator auto () -> int;
  operator auto*() -> int;
};

template <typename> struct B
{
  enum { e };
};

template <typename T> struct C
{
  C () : i () {}
  int i;
};

bool d = (auto (A::*)()) 0;

void
foo ()
{
  __extension__ (auto) { 0 };
  C<int> c;
  dynamic_cast<auto> (c);
  reinterpret_cast<auto> (c);
  int i = auto (0);
  auto p1 = new (auto);
  auto p2 = new (auto) (42);
  
# 51 "./cpp0x/auto9.C" 3 4
 __builtin_offsetof (
# 51 "./cpp0x/auto9.C"
 auto
# 51 "./cpp0x/auto9.C" 3 4
 , 
# 51 "./cpp0x/auto9.C"
 fld
# 51 "./cpp0x/auto9.C" 3 4
 )
# 51 "./cpp0x/auto9.C"
                     ;
  
# 52 "./cpp0x/auto9.C" 3 4
 __builtin_offsetof (
# 52 "./cpp0x/auto9.C"
 auto *
# 52 "./cpp0x/auto9.C" 3 4
 , 
# 52 "./cpp0x/auto9.C"
 fld
# 52 "./cpp0x/auto9.C" 3 4
 )
# 52 "./cpp0x/auto9.C"
                       ;
  sizeof (auto);
  sizeof (auto *);
}

void
foo2 (void)
{
  __alignof__ (auto);
  __alignof__ (auto *);
  __typeof__ (auto) v1;
  __typeof__ (auto *) v2;
  __is_class (auto);
  __is_pod (auto *);
  __is_base_of (int, auto);
  __is_base_of (auto, int);
  __is_base_of (auto, auto *);
}

B<auto> b;
C<auto> c;
C<auto *> c2;

enum : auto { EE = 0 };
enum struct D : auto * { FF = 0 };

void
bar ()
{
  try { } catch (auto i) { }
  try { } catch (auto) { }
  try { } catch (auto *i) { }
  try { } catch (auto *) { }
}

void
baz (int i, ...)
{
  va_list ap;
  
# 91 "./cpp0x/auto9.C" 3 4
 __builtin_va_start(
# 91 "./cpp0x/auto9.C"
 ap
# 91 "./cpp0x/auto9.C" 3 4
 ,
# 91 "./cpp0x/auto9.C"
 i
# 91 "./cpp0x/auto9.C" 3 4
 )
# 91 "./cpp0x/auto9.C"
                 ;
  
# 92 "./cpp0x/auto9.C" 3 4
 __builtin_va_arg(
# 92 "./cpp0x/auto9.C"
 ap
# 92 "./cpp0x/auto9.C" 3 4
 ,
# 92 "./cpp0x/auto9.C"
 auto
# 92 "./cpp0x/auto9.C" 3 4
 )
# 92 "./cpp0x/auto9.C"
                  ;
  
# 93 "./cpp0x/auto9.C" 3 4
 __builtin_va_arg(
# 93 "./cpp0x/auto9.C"
 ap
# 93 "./cpp0x/auto9.C" 3 4
 ,
# 93 "./cpp0x/auto9.C"
 auto *
# 93 "./cpp0x/auto9.C" 3 4
 )
# 93 "./cpp0x/auto9.C"
                    ;
  
# 94 "./cpp0x/auto9.C" 3 4
 __builtin_va_arg(
# 94 "./cpp0x/auto9.C"
 ap
# 94 "./cpp0x/auto9.C" 3 4
 ,
# 94 "./cpp0x/auto9.C"
 auto &
# 94 "./cpp0x/auto9.C" 3 4
 )
# 94 "./cpp0x/auto9.C"
                    ;
  
# 95 "./cpp0x/auto9.C" 3 4
 __builtin_va_end(
# 95 "./cpp0x/auto9.C"
 ap
# 95 "./cpp0x/auto9.C" 3 4
 )
# 95 "./cpp0x/auto9.C"
            ;
}

template <typename T = auto> struct E {};
template <class T = auto *> struct F {};

auto fnlate () -> auto;
auto fnlate2 () -> auto *;

void
badthrow () throw (auto)
{
}

void
badthrow2 () throw (auto &)
{
}

template <auto V = 4> struct G {};

template <typename T> struct H { H (); ~H (); };
H<auto> h;

void qq (auto);
void qr (auto*);


typedef auto autot;
