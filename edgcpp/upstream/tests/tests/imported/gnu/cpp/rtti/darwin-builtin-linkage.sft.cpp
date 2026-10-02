//type: fp
//options: 
# 0 "./rtti/darwin-builtin-linkage.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./rtti/darwin-builtin-linkage.C"






# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cxxabi.h" 1 3
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cxxabi.h" 3
#pragma GCC visibility push(default)

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
# 51 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cxxabi.h" 2 3
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
# 941 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/pstl/pstl_config.h" 1 3
# 942 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3



#pragma GCC diagnostic pop
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cxxabi.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/cxxabi_tweaks.h" 1 3
# 34 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/cxxabi_tweaks.h" 3
namespace __cxxabiv1
{
  extern "C"
  {
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/cxxabi_tweaks.h" 3
  __extension__ typedef int __guard __attribute__((mode (__DI__)));


  typedef void __cxa_vec_ctor_return_type;


  typedef void __cxa_cdtor_return_type;


  }
}
# 53 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cxxabi.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cxxabi_forced.h" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cxxabi_forced.h" 3
#pragma GCC visibility push(default)


namespace __cxxabiv1
{







  class __forced_unwind
  {
    virtual ~__forced_unwind() throw();


    virtual void __pure_dummy() = 0;
  };
}


#pragma GCC visibility pop
# 54 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cxxabi.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cxxabi_init_exception.h" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cxxabi_init_exception.h" 3
#pragma GCC visibility push(default)

# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cxxabi_init_exception.h" 2 3
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cxxabi_init_exception.h" 3
namespace std
{
  class type_info;
}

namespace __cxxabiv1
{
  struct __cxa_refcounted_exception;

  extern "C"
    {

      void*
      __cxa_allocate_exception(size_t) noexcept;

      void
      __cxa_free_exception(void*) noexcept;


      __cxa_refcounted_exception*
      __cxa_init_primary_exception(void *__object, std::type_info *__tinfo,
                void ( *__dest) (void *))
 noexcept;

    }
}



#pragma GCC visibility pop
# 55 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cxxabi.h" 2 3


namespace __cxxabiv1
{
  extern "C"
  {


  typedef __cxa_cdtor_return_type (*__cxa_cdtor_type)(void *);


  void*
  __cxa_vec_new(size_t __element_count, size_t __element_size,
  size_t __padding_size, __cxa_cdtor_type __constructor,
  __cxa_cdtor_type __destructor);

  void*
  __cxa_vec_new2(size_t __element_count, size_t __element_size,
   size_t __padding_size, __cxa_cdtor_type __constructor,
   __cxa_cdtor_type __destructor, void *(*__alloc) (size_t),
   void (*__dealloc) (void*));

  void*
  __cxa_vec_new3(size_t __element_count, size_t __element_size,
   size_t __padding_size, __cxa_cdtor_type __constructor,
   __cxa_cdtor_type __destructor, void *(*__alloc) (size_t),
   void (*__dealloc) (void*, size_t));


  __cxa_vec_ctor_return_type
  __cxa_vec_ctor(void* __array_address, size_t __element_count,
   size_t __element_size, __cxa_cdtor_type __constructor,
   __cxa_cdtor_type __destructor);

  __cxa_vec_ctor_return_type
  __cxa_vec_cctor(void* __dest_array, void* __src_array,
    size_t __element_count, size_t __element_size,
    __cxa_cdtor_return_type (*__constructor) (void*, void*),
    __cxa_cdtor_type __destructor);


  void
  __cxa_vec_dtor(void* __array_address, size_t __element_count,
   size_t __element_size, __cxa_cdtor_type __destructor);

  void
  __cxa_vec_cleanup(void* __array_address, size_t __element_count, size_t __s,
      __cxa_cdtor_type __destructor) noexcept;


  void
  __cxa_vec_delete(void* __array_address, size_t __element_size,
     size_t __padding_size, __cxa_cdtor_type __destructor);

  void
  __cxa_vec_delete2(void* __array_address, size_t __element_size,
      size_t __padding_size, __cxa_cdtor_type __destructor,
      void (*__dealloc) (void*));

  void
  __cxa_vec_delete3(void* __array_address, size_t __element_size,
      size_t __padding_size, __cxa_cdtor_type __destructor,
      void (*__dealloc) (void*, size_t));

  int
  __cxa_guard_acquire(__guard*);

  void
  __cxa_guard_release(__guard*) noexcept;

  void
  __cxa_guard_abort(__guard*) noexcept;


  int

  __cxa_atexit(void ( *)(void*), void*, void*) noexcept;




  void
  __cxa_finalize(void*);


  int

  __cxa_thread_atexit(void ( *)(void*), void*, void *) noexcept;





  void
  __cxa_pure_virtual(void) __attribute__ ((__noreturn__));

  void
  __cxa_deleted_virtual(void) __attribute__ ((__noreturn__));


  void
  __cxa_bad_cast() __attribute__((__noreturn__));

  void
  __cxa_bad_typeid() __attribute__((__noreturn__));

  void
  __cxa_throw_bad_array_new_length() __attribute__((__noreturn__));
# 205 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cxxabi.h" 3
  char*
  __cxa_demangle(const char* __mangled_name, char* __output_buffer,
   size_t* __length, int* __status);


  }
}




# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/typeinfo" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 3
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
# 217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cxxabi.h" 2 3

namespace __cxxabiv1
{

  class __fundamental_type_info : public std::type_info
  {
  public:
    explicit
    __fundamental_type_info(const char* __n) : std::type_info(__n) { }

    virtual
    ~__fundamental_type_info();
  };


  class __array_type_info : public std::type_info
  {
  public:
    explicit
    __array_type_info(const char* __n) : std::type_info(__n) { }

    virtual
    ~__array_type_info();
  };


  class __function_type_info : public std::type_info
  {
  public:
    explicit
    __function_type_info(const char* __n) : std::type_info(__n) { }

    virtual
    ~__function_type_info();

  protected:

    virtual bool
    __is_function_p() const;
  };


  class __enum_type_info : public std::type_info
  {
  public:
    explicit
    __enum_type_info(const char* __n) : std::type_info(__n) { }

    virtual
    ~__enum_type_info();
  };


  class __pbase_type_info : public std::type_info
  {
  public:
    unsigned int __flags;
    const std::type_info* __pointee;

    explicit
    __pbase_type_info(const char* __n, int __quals,
        const std::type_info* __type)
    : std::type_info(__n), __flags(__quals), __pointee(__type)
    { }

    virtual
    ~__pbase_type_info();


    enum __masks
      {
 __const_mask = 0x1,
 __volatile_mask = 0x2,
 __restrict_mask = 0x4,
 __incomplete_mask = 0x8,
 __incomplete_class_mask = 0x10,
 __transaction_safe_mask = 0x20,
 __noexcept_mask = 0x40
      };

  protected:
    __pbase_type_info(const __pbase_type_info&);

    __pbase_type_info&
    operator=(const __pbase_type_info&);


    virtual bool
    __do_catch(const std::type_info* __thr_type, void** __thr_obj,
        unsigned int __outer) const;

    inline virtual bool
    __pointer_catch(const __pbase_type_info* __thr_type, void** __thr_obj,
      unsigned __outer) const;
  };

  inline bool __pbase_type_info::
  __pointer_catch (const __pbase_type_info *thrown_type,
     void **thr_obj,
     unsigned outer) const
  {
    return __pointee->__do_catch (thrown_type->__pointee, thr_obj, outer + 2);
  }


  class __pointer_type_info : public __pbase_type_info
  {
  public:
    explicit
    __pointer_type_info(const char* __n, int __quals,
   const std::type_info* __type)
    : __pbase_type_info (__n, __quals, __type) { }


    virtual
    ~__pointer_type_info();

  protected:

    virtual bool
    __is_pointer_p() const;

    virtual bool
    __pointer_catch(const __pbase_type_info* __thr_type, void** __thr_obj,
      unsigned __outer) const;
  };

  class __class_type_info;


  class __pointer_to_member_type_info : public __pbase_type_info
  {
  public:
    __class_type_info* __context;

    explicit
    __pointer_to_member_type_info(const char* __n, int __quals,
      const std::type_info* __type,
      __class_type_info* __klass)
    : __pbase_type_info(__n, __quals, __type), __context(__klass) { }

    virtual
    ~__pointer_to_member_type_info();

  protected:
    __pointer_to_member_type_info(const __pointer_to_member_type_info&);

    __pointer_to_member_type_info&
    operator=(const __pointer_to_member_type_info&);


    virtual bool
    __pointer_catch(const __pbase_type_info* __thr_type, void** __thr_obj,
      unsigned __outer) const;
  };


  class __base_class_type_info
  {
  public:
    const __class_type_info* __base_type;



    long __offset_flags;


    enum __offset_flags_masks
      {
 __virtual_mask = 0x1,
 __public_mask = 0x2,
 __hwm_bit = 2,
 __offset_shift = 8
      };


    bool
    __is_virtual_p() const
    { return __offset_flags & __virtual_mask; }

    bool
    __is_public_p() const
    { return __offset_flags & __public_mask; }

    ptrdiff_t
    __offset() const
    {



      return static_cast<ptrdiff_t>(__offset_flags) >> __offset_shift;
    }
  };


  class __class_type_info : public std::type_info
  {
  public:
    explicit
    __class_type_info (const char *__n) : type_info(__n) { }

    virtual
    ~__class_type_info ();






    enum __sub_kind
      {

 __unknown = 0,



 __not_contained,


 __contained_ambig,


 __contained_virtual_mask = __base_class_type_info::__virtual_mask,


 __contained_public_mask = __base_class_type_info::__public_mask,


 __contained_mask = 1 << __base_class_type_info::__hwm_bit,

 __contained_private = __contained_mask,
 __contained_public = __contained_mask | __contained_public_mask
      };

    struct __upcast_result;
    struct __dyncast_result;

  protected:

    virtual bool
    __do_upcast(const __class_type_info* __dst_type, void**__obj_ptr) const;

    virtual bool
    __do_catch(const type_info* __thr_type, void** __thr_obj,
        unsigned __outer) const;

  public:


    virtual bool
    __do_upcast(const __class_type_info* __dst, const void* __obj,
  __upcast_result& __restrict __result) const;







    inline __sub_kind
    __find_public_src(ptrdiff_t __src2dst, const void* __obj_ptr,
        const __class_type_info* __src_type,
        const void* __src_ptr) const;
# 488 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cxxabi.h" 3
    virtual bool
    __do_dyncast(ptrdiff_t __src2dst, __sub_kind __access_path,
   const __class_type_info* __dst_type, const void* __obj_ptr,
   const __class_type_info* __src_type, const void* __src_ptr,
   __dyncast_result& __result) const;





    virtual __sub_kind
    __do_find_public_src(ptrdiff_t __src2dst, const void* __obj_ptr,
    const __class_type_info* __src_type,
    const void* __src_ptr) const;
  };


  class __si_class_type_info : public __class_type_info
  {
  public:
    const __class_type_info* __base_type;

    explicit
    __si_class_type_info(const char *__n, const __class_type_info *__base)
    : __class_type_info(__n), __base_type(__base) { }

    virtual
    ~__si_class_type_info();

  protected:
    __si_class_type_info(const __si_class_type_info&);

    __si_class_type_info&
    operator=(const __si_class_type_info&);


    virtual bool
    __do_dyncast(ptrdiff_t __src2dst, __sub_kind __access_path,
   const __class_type_info* __dst_type, const void* __obj_ptr,
   const __class_type_info* __src_type, const void* __src_ptr,
   __dyncast_result& __result) const;

    virtual __sub_kind
    __do_find_public_src(ptrdiff_t __src2dst, const void* __obj_ptr,
    const __class_type_info* __src_type,
    const void* __sub_ptr) const;

    virtual bool
    __do_upcast(const __class_type_info*__dst, const void*__obj,
  __upcast_result& __restrict __result) const;
  };


  class __vmi_class_type_info : public __class_type_info
  {
  public:
    unsigned int __flags;
    unsigned int __base_count;




    __base_class_type_info __base_info[1];

    explicit
    __vmi_class_type_info(const char* __n, int ___flags)
    : __class_type_info(__n), __flags(___flags), __base_count(0) { }

    virtual
    ~__vmi_class_type_info();


    enum __flags_masks
      {
 __non_diamond_repeat_mask = 0x1,
 __diamond_shaped_mask = 0x2,
 __flags_unknown_mask = 0x10
      };

  protected:

    virtual bool
    __do_dyncast(ptrdiff_t __src2dst, __sub_kind __access_path,
   const __class_type_info* __dst_type, const void* __obj_ptr,
   const __class_type_info* __src_type, const void* __src_ptr,
   __dyncast_result& __result) const;

    virtual __sub_kind
    __do_find_public_src(ptrdiff_t __src2dst, const void* __obj_ptr,
    const __class_type_info* __src_type,
    const void* __src_ptr) const;

    virtual bool
    __do_upcast(const __class_type_info* __dst, const void* __obj,
  __upcast_result& __restrict __result) const;
  };


  struct __cxa_exception;
  struct __cxa_refcounted_exception;
  struct __cxa_dependent_exception;
  struct __cxa_eh_globals;

  extern "C"
  {
# 601 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cxxabi.h" 3
  void*
  __dynamic_cast(const void* __src_ptr,
   const __class_type_info* __src_type,
   const __class_type_info* __dst_type,
   ptrdiff_t __src2dst);
# 614 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cxxabi.h" 3
  __cxa_eh_globals*
  __cxa_get_globals() noexcept __attribute__ ((__const__));

  __cxa_eh_globals*
  __cxa_get_globals_fast() noexcept __attribute__ ((__const__));


  void
  __cxa_free_exception(void*) noexcept;


  void
  __cxa_throw(void*, std::type_info*, void ( *) (void *))
  __attribute__((__noreturn__));


  void*
  __cxa_get_exception_ptr(void*) noexcept __attribute__ ((__pure__));

  void*
  __cxa_begin_catch(void*) noexcept;

  void
  __cxa_end_catch();

  void
  __cxa_rethrow() __attribute__((__noreturn__));



  std::type_info*
  __cxa_current_exception_type() noexcept __attribute__ ((__pure__));




  __cxa_dependent_exception*
  __cxa_allocate_dependent_exception() noexcept;


  void
  __cxa_free_dependent_exception(__cxa_dependent_exception*) noexcept;

  }



  class __foreign_exception
  {
    virtual ~__foreign_exception() throw();
    virtual void __pure_dummy() = 0;
  };

}
# 689 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cxxabi.h" 3
namespace abi = __cxxabiv1;

namespace __gnu_cxx
{
# 706 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cxxabi.h" 3
  class recursive_init_error: public std::exception
  {
  public:
    recursive_init_error() noexcept;
    virtual ~recursive_init_error() noexcept;
  };
}


#pragma GCC visibility pop
# 8 "./rtti/darwin-builtin-linkage.C" 2


# 9 "./rtti/darwin-builtin-linkage.C"
namespace __cxxabiv1 {

using namespace std;




__fundamental_type_info::
~__fundamental_type_info ()
{}

}
