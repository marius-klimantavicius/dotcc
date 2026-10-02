//type: fn
//options: --c++03
# 0 "./lookup/missing-std-include-8.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lookup/missing-std-include-8.C"




# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 1 3
# 65 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/memoryfwd.h" 1 3
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/memoryfwd.h" 3
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





#pragma GCC visibility push(default)


  extern "C++" __attribute__ ((__noreturn__, __always_inline__))
  inline void __terminate() throw()
  {
    void terminate() throw() __attribute__ ((__noreturn__,__cold__));
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
  inline bool
  __is_constant_evaluated()
  {







    return false;

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
  ;
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
# 51 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/memoryfwd.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/memoryfwd.h" 3
  template<typename>
    class allocator;

  template<>
    class allocator<void>;
# 83 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/memoryfwd.h" 3

}
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 1 3
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++allocator.h" 1 3
# 33 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++allocator.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 1 3
# 34 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 1 3
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 3
extern "C++" {

namespace std __attribute__ ((__visibility__ ("default")))
{
# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 3
  class exception
  {
  public:
    exception() throw() { }
    virtual ~exception() throw();
# 75 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 3
    virtual const char*
    what() const throw();
  };



}

}
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 2 3





# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 2 3

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"

#pragma GCC visibility push(default)

extern "C++" {

namespace std
{






  class bad_alloc : public exception
  {
  public:
    bad_alloc() throw() { }
# 78 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
    virtual ~bad_alloc() throw();


    virtual const char* what() const throw();
  };
# 103 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
  struct nothrow_t
  {



  };

  extern const nothrow_t nothrow;



  typedef void (*new_handler)();



  new_handler set_new_handler(new_handler) throw();





}
# 137 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
 void* operator new(std::size_t)
  throw(std::bad_alloc)
  __attribute__((__externally_visible__, __malloc__));
 void* operator new[](std::size_t)
  throw(std::bad_alloc)
  __attribute__((__externally_visible__, __malloc__));
void operator delete(void*) throw()
  __attribute__((__externally_visible__));
void operator delete[](void*) throw()
  __attribute__((__externally_visible__));
# 155 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
 void* operator new(std::size_t, const std::nothrow_t&)
  throw()
  __attribute__((__externally_visible__, __alloc_size__ (1), __malloc__));
 void* operator new[](std::size_t, const std::nothrow_t&)
  throw()
  __attribute__((__externally_visible__, __alloc_size__ (1), __malloc__));
void operator delete(void*, const std::nothrow_t&)
  throw()
  __attribute__((__externally_visible__));
void operator delete[](void*, const std::nothrow_t&)
  throw()
  __attribute__((__externally_visible__));
# 205 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
 inline
void* operator new(std::size_t, void* __p)
  throw()
{ return __p; }
 inline
void* operator new[](std::size_t, void* __p)
  throw()
{ return __p; }




inline void operator delete (void*, void*)
  throw()
{ }
inline void operator delete[](void*, void*)
  throw()
{ }

}

namespace std
{
# 264 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
}

#pragma GCC visibility pop
#pragma GCC diagnostic pop
# 35 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functexcept.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functexcept.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_defines.h" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functexcept.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{




  void
  __throw_bad_exception(void) __attribute__((__noreturn__));


  void
  __throw_bad_alloc(void) __attribute__((__noreturn__));

  void
  __throw_bad_array_new_length(void) __attribute__((__noreturn__));


  void
  __throw_bad_cast(void) __attribute__((__noreturn__,__cold__));

  void
  __throw_bad_typeid(void) __attribute__((__noreturn__,__cold__));


  void
  __throw_logic_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_domain_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_invalid_argument(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_length_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_out_of_range(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_out_of_range_fmt(const char*, ...) __attribute__((__noreturn__,__cold__))
    __attribute__((__format__(__gnu_printf__, 1, 2)));

  void
  __throw_runtime_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_range_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_overflow_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_underflow_error(const char*) __attribute__((__noreturn__,__cold__));


  void
  __throw_ios_failure(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_ios_failure(const char*, int) __attribute__((__noreturn__,__cold__));


  void
  __throw_system_error(int) __attribute__((__noreturn__,__cold__));


  void
  __throw_future_error(int) __attribute__((__noreturn__,__cold__));


  void
  __throw_bad_function_call() __attribute__((__noreturn__,__cold__));
# 140 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functexcept.h" 3

}
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 1 3
# 35 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/concept_check.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/concept_check.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wvariadic-macros"
# 86 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/concept_check.h" 3
#pragma GCC diagnostic pop
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 2 3




namespace std __attribute__ ((__visibility__ ("default")))
{







  template<typename _Tp>
    __attribute__((__always_inline__))
    inline _Tp*
    __addressof(_Tp& __r)
    { return __builtin_addressof(__r); }
# 217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
   
    inline





    void

    swap(_Tp& __a, _Tp& __b)
   

    {


     

      _Tp __tmp = (__a);
      __a = (__b);
      __b = (__tmp);
    }




  template<typename _Tp, size_t _Nm>
   
    inline



    void

    swap(_Tp (&__a)[_Nm], _Tp (&__b)[_Nm])
   
    {
      for (size_t __n = 0; __n < _Nm; ++__n)
 swap(__a[__n], __b[__n]);
    }



}
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 2 3




namespace std __attribute__ ((__visibility__ ("default")))
{

# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
  template<typename _Tp>
    class __new_allocator
    {
    public:
      typedef _Tp value_type;
      typedef std::size_t size_type;
      typedef std::ptrdiff_t difference_type;

      typedef _Tp* pointer;
      typedef const _Tp* const_pointer;
      typedef _Tp& reference;
      typedef const _Tp& const_reference;

      template<typename _Tp1>
 struct rebind
 { typedef __new_allocator<_Tp1> other; };
# 86 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
      __attribute__((__always_inline__))
     
      __new_allocator() throw() { }

      __attribute__((__always_inline__))
     
      __new_allocator(const __new_allocator&) throw() { }

      template<typename _Tp1>
 __attribute__((__always_inline__))

 __new_allocator(const __new_allocator<_Tp1>&) throw() { }






      ~__new_allocator() throw() { }

      pointer
      address(reference __x) const
      { return std::__addressof(__x); }

      const_pointer
      address(const_reference __x) const
      { return std::__addressof(__x); }
# 125 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
      _Tp*
      allocate(size_type __n, const void* = static_cast<const void*>(0))
      {






 if (__builtin_expect(__n > this->_M_max_size(), false))
   {


     if (__n > (std::size_t(-1) / sizeof(_Tp)))
       std::__throw_bad_array_new_length();
     std::__throw_bad_alloc();
   }
# 151 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
 return static_cast<_Tp*>(__builtin_operator_new(__n * sizeof(_Tp)));
      }


      void
      deallocate(_Tp* __p, size_type __n __attribute__ ((__unused__)))
      {
# 172 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
 __builtin_operator_delete((__p));
      }






      __attribute__((__always_inline__))
      size_type
      max_size() const throw()
      { return _M_max_size(); }
# 202 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
      __attribute__((__always_inline__))
      void
      construct(pointer __p, const _Tp& __val)
      { ::new((void *)__p) _Tp(__val); }

      __attribute__((__always_inline__))
      void
      destroy(pointer __p) { __p->~_Tp(); }



      template<typename _Up>
 friend __attribute__((__always_inline__)) bool
 operator==(const __new_allocator&, const __new_allocator<_Up>&)
 throw()
 { return true; }


      template<typename _Up>
 friend __attribute__((__always_inline__)) bool
 operator!=(const __new_allocator&, const __new_allocator<_Up>&)
 throw()
 { return false; }


    private:
      __attribute__((__always_inline__))
      size_type
      _M_max_size() const throw()
      {

 return std::size_t(0x7fffffffffffffffL) / sizeof(_Tp);



      }
    };


}
# 34 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++allocator.h" 2 3
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 2 3





#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"

namespace std __attribute__ ((__visibility__ ("default")))
{

# 75 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
  template<>
    class allocator<void>
    {
    public:
      typedef void value_type;
      typedef size_t size_type;
      typedef ptrdiff_t difference_type;



      typedef void* pointer;
      typedef const void* const_pointer;

      template<typename _Tp1>
 struct rebind
 { typedef allocator<_Tp1> other; };
# 120 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
    };
# 132 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
  template<typename _Tp>
    class allocator : public __new_allocator<_Tp>
    {
    public:
      typedef _Tp value_type;
      typedef size_t size_type;
      typedef ptrdiff_t difference_type;



      typedef _Tp* pointer;
      typedef const _Tp* const_pointer;
      typedef _Tp& reference;
      typedef const _Tp& const_reference;

      template<typename _Tp1>
 struct rebind
 { typedef allocator<_Tp1> other; };
# 166 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
      __attribute__((__always_inline__))
     
      allocator() throw() { }

      __attribute__((__always_inline__))
     
      allocator(const allocator& __a) throw()
      : __new_allocator<_Tp>(__a) { }






      template<typename _Tp1>
 __attribute__((__always_inline__))

 allocator(const allocator<_Tp1>&) throw() { }

      __attribute__((__always_inline__))



      ~allocator() throw() { }
# 219 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
      friend __attribute__((__always_inline__))
      bool
      operator==(const allocator&, const allocator&) throw()
      { return true; }


      friend __attribute__((__always_inline__))
      bool
      operator!=(const allocator&, const allocator&) throw()
      { return false; }



    };






  template<typename _T1, typename _T2>
    __attribute__((__always_inline__))
    inline bool
    operator==(const allocator<_T1>&, const allocator<_T2>&)
    throw()
    { return true; }


  template<typename _T1, typename _T2>
    __attribute__((__always_inline__))
    inline bool
    operator!=(const allocator<_T1>&, const allocator<_T2>&)
    throw()
    { return false; }






  template<typename _Tp>
    class allocator<const _Tp>
    {
    public:
      typedef _Tp value_type;
      allocator() { }
      template<typename _Up> allocator(const allocator<_Up>&) { }
    };

  template<typename _Tp>
    class allocator<volatile _Tp>
    {
    public:
      typedef _Tp value_type;
      allocator() { }
      template<typename _Up> allocator(const allocator<_Up>&) { }
    };

  template<typename _Tp>
    class allocator<const volatile _Tp>
    {
    public:
      typedef _Tp value_type;
      allocator() { }
      template<typename _Up> allocator(const allocator<_Up>&) { }
    };







  extern template class allocator<char>;
  extern template class allocator<wchar_t>;






}

#pragma GCC diagnostic pop
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 1 3
# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 1 3
# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 1 3
# 76 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 95 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  struct input_iterator_tag { };


  struct output_iterator_tag { };


  struct forward_iterator_tag : public input_iterator_tag { };



  struct bidirectional_iterator_tag : public forward_iterator_tag { };



  struct random_access_iterator_tag : public bidirectional_iterator_tag { };
# 127 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _Category, typename _Tp, typename _Distance = ptrdiff_t,
           typename _Pointer = _Tp*, typename _Reference = _Tp&>
    struct iterator
    {

      typedef _Category iterator_category;

      typedef _Tp value_type;

      typedef _Distance difference_type;

      typedef _Pointer pointer;

      typedef _Reference reference;
    };
# 151 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _Iterator>
    struct iterator_traits;
# 183 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _Iterator>
    struct iterator_traits
    {
      typedef typename _Iterator::iterator_category iterator_category;
      typedef typename _Iterator::value_type value_type;
      typedef typename _Iterator::difference_type difference_type;
      typedef typename _Iterator::pointer pointer;
      typedef typename _Iterator::reference reference;
    };
# 211 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _Tp>
    struct iterator_traits<_Tp*>
    {
      typedef random_access_iterator_tag iterator_category;
      typedef _Tp value_type;
      typedef ptrdiff_t difference_type;
      typedef _Tp* pointer;
      typedef _Tp& reference;
    };


  template<typename _Tp>
    struct iterator_traits<const _Tp*>
    {
      typedef random_access_iterator_tag iterator_category;
      typedef _Tp value_type;
      typedef ptrdiff_t difference_type;
      typedef const _Tp* pointer;
      typedef const _Tp& reference;
    };






  template<typename _Iter>
    __attribute__((__always_inline__))
    inline
    typename iterator_traits<_Iter>::iterator_category
    __iterator_category(const _Iter&)
    { return typename iterator_traits<_Iter>::iterator_category(); }
# 271 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _It, typename _Traits = iterator_traits<_It>,
    typename _Cat = typename _Traits::iterator_category>
    struct __is_random_access_iter
    { enum { __value = __is_base_of(random_access_iterator_tag, _Cat) }; };



}
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 1 3
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/debug/assertions.h" 1 3
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 2 3


namespace std __attribute__ ((__visibility__ ("default")))
{




  template <typename> struct _List_iterator;
  template <typename> struct _List_const_iterator;


  template<typename _InputIterator>
    inline
    typename iterator_traits<_InputIterator>::difference_type
    __distance(_InputIterator __first, _InputIterator __last,
               input_iterator_tag)
    {

     

      typename iterator_traits<_InputIterator>::difference_type __n = 0;
      while (__first != __last)
 {
   ++__first;
   ++__n;
 }
      return __n;
    }

  template<typename _RandomAccessIterator>
    __attribute__((__always_inline__))
    inline
    typename iterator_traits<_RandomAccessIterator>::difference_type
    __distance(_RandomAccessIterator __first, _RandomAccessIterator __last,
               random_access_iterator_tag)
    {

     

      return __last - __first;
    }



  template<typename _Tp>
    ptrdiff_t
    __distance(std::_List_iterator<_Tp>,
        std::_List_iterator<_Tp>,
        input_iterator_tag);

  template<typename _Tp>
    ptrdiff_t
    __distance(std::_List_const_iterator<_Tp>,
        std::_List_const_iterator<_Tp>,
        input_iterator_tag);
# 146 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 3
  template<typename _InputIterator>
    __attribute__((__always_inline__))
    inline
    typename iterator_traits<_InputIterator>::difference_type
    distance(_InputIterator __first, _InputIterator __last)
    {

      return std::__distance(__first, __last,
        std::__iterator_category(__first));
    }

  template<typename _InputIterator, typename _Distance>
    inline void
    __advance(_InputIterator& __i, _Distance __n, input_iterator_tag)
    {

     
      do { if (__builtin_expect(!bool(__n >= 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h", 163, __PRETTY_FUNCTION__, "__n >= 0"); } while (false);
      while (__n--)
 ++__i;
    }

  template<typename _BidirectionalIterator, typename _Distance>
    inline void
    __advance(_BidirectionalIterator& __i, _Distance __n,
       bidirectional_iterator_tag)
    {

     

      if (__n > 0)
        while (__n--)
   ++__i;
      else
        while (__n++)
   --__i;
    }

  template<typename _RandomAccessIterator, typename _Distance>
    inline void
    __advance(_RandomAccessIterator& __i, _Distance __n,
              random_access_iterator_tag)
    {

     

      if (__builtin_constant_p(__n) && __n == 1)
 ++__i;
      else if (__builtin_constant_p(__n) && __n == -1)
 --__i;
      else
 __i += __n;
    }
# 219 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 3
  template<typename _InputIterator, typename _Distance>
    __attribute__((__always_inline__))
    inline void
    advance(_InputIterator& __i, _Distance __n)
    {

      typename iterator_traits<_InputIterator>::difference_type __d = __n;
      std::__advance(__i, __d, std::__iterator_category(__i));
    }
# 258 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 3

}
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 2 3
# 73 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 136 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
  template<typename _T1, typename _T2>
    inline void
    _Construct(_T1* __p, const _T2& __value)
    {


      ::new(static_cast<void*>(__p)) _T1(__value);
    }


  template<typename _T1>
    inline void
    _Construct_novalue(_T1* __p)
    { ::new(static_cast<void*>(__p)) _T1; }

  template<typename _ForwardIterator>
    void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last);




  template<typename _Tp>
    inline void
    _Destroy(_Tp* __pointer)
    {



      __pointer->~_Tp();

    }

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"


  template<bool>
    struct _Destroy_aux
    {
      template<typename _ForwardIterator>
 static void
 __destroy(_ForwardIterator __first, _ForwardIterator __last)
 {
   for (; __first != __last; ++__first)
     std::_Destroy(std::__addressof(*__first));
 }
    };

  template<>
    struct _Destroy_aux<true>
    {
      template<typename _ForwardIterator>
        static void
        __destroy(_ForwardIterator, _ForwardIterator) { }
    };







  template<typename _ForwardIterator>
    inline void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
                       _Value_type;
# 218 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
      std::_Destroy_aux<__has_trivial_destructor(_Value_type)>::
 __destroy(__first, __last);

    }


  template<bool>
    struct _Destroy_n_aux
    {
      template<typename _ForwardIterator, typename _Size>
 static _ForwardIterator
 __destroy_n(_ForwardIterator __first, _Size __count)
 {
   for (; __count > 0; (void)++__first, --__count)
     std::_Destroy(std::__addressof(*__first));
   return __first;
 }
    };

  template<>
    struct _Destroy_n_aux<true>
    {
      template<typename _ForwardIterator, typename _Size>
        static _ForwardIterator
        __destroy_n(_ForwardIterator __first, _Size __count)
 {
   std::advance(__first, __count);
   return __first;
 }
    };







  template<typename _ForwardIterator, typename _Size>
    inline _ForwardIterator
    _Destroy_n(_ForwardIterator __first, _Size __count)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
                       _Value_type;
# 277 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
      return std::_Destroy_n_aux<__has_trivial_destructor(_Value_type)>::
 __destroy_n(__first, __count);

    }
#pragma GCC diagnostic pop
# 299 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3

}
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 1 3
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 260 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _U1, typename _U2> class __pair_base
  {







  };
# 283 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    struct pair
    : public __pair_base<_T1, _T2>
    {
      typedef _T1 first_type;
      typedef _T2 second_type;

      _T1 first;
      _T2 second;
# 973 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
      pair() : first(), second() { }


      pair(const _T1& __a, const _T2& __b)
      : first(__a), second(__b) { }


      template<typename _U1, typename _U2>
 pair(const pair<_U1, _U2>& __p)
 : first(__p.first), second(__p.second)
 {

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-local-typedefs"
   typedef int _DanglingCheck1[
     __reference_constructs_from_temporary(_T1, const _U1&) ? -1 : 1
        ];
   typedef int _DanglingCheck2[
     __reference_constructs_from_temporary(_T2, const _U2&) ? -1 : 1
        ];
#pragma GCC diagnostic pop

 }

    };
# 1039 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
   
    inline bool
    operator==(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return __x.first == __y.first && __x.second == __y.second; }
# 1052 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
   
    inline bool
    operator<(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return __x.first < __y.first
      || (!(__y.first < __x.first) && __x.second < __y.second); }


  template<typename _T1, typename _T2>
   
    inline bool
    operator!=(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return !(__x == __y); }


  template<typename _T1, typename _T2>
   
    inline bool
    operator>(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return __y < __x; }


  template<typename _T1, typename _T2>
   
    inline bool
    operator<=(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return !(__y < __x); }


  template<typename _T1, typename _T2>
   
    inline bool
    operator>=(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return !(__x < __y); }
# 1154 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    inline pair<_T1, _T2>
    make_pair(_T1 __x, _T2 __y)
    { return pair<_T1, _T2>(__x, __y); }
# 1347 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3

}
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 2 3




#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
# 76 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
extern "C++" {

namespace std __attribute__ ((__visibility__ ("default")))
{


  struct __true_type { };
  struct __false_type { };

  template<bool>
    struct __truth_type
    { typedef __false_type __type; };

  template<>
    struct __truth_type<true>
    { typedef __true_type __type; };



  template<class _Sp, class _Tp>
    struct __traitor
    {
      enum { __value = bool(_Sp::__value) || bool(_Tp::__value) };
      typedef typename __truth_type<__value>::__type __type;
    };


  template<typename, typename>
    struct __are_same
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };

  template<typename _Tp>
    struct __are_same<_Tp, _Tp>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };




  template<typename _Tp>
    struct __is_integer
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };



  template<>
    struct __is_integer<bool>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<signed char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };


  template<>
    struct __is_integer<wchar_t>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };
# 191 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<>
    struct __is_integer<short>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned short>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<int>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned int>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<long>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned long>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<long long>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned long long>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };
# 281 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Tp>
    struct __is_floating
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };


  template<>
    struct __is_floating<float>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_floating<double>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_floating<long double>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };
# 358 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Tp>
    struct __is_arithmetic
    : public __traitor<__is_integer<_Tp>, __is_floating<_Tp> >
    { };




  template<typename _Tp>
    struct __is_char
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };

  template<>
    struct __is_char<char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };


  template<>
    struct __is_char<wchar_t>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };


  template<typename _Tp>
    struct __is_byte
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };

  template<>
    struct __is_byte<char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_byte<signed char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_byte<unsigned char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };
# 438 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Tp>
    struct __is_nonvolatile_trivially_copyable
    {
      enum { __value = __is_trivially_copyable(_Tp) };
    };




  template<typename _Tp>
    struct __is_nonvolatile_trivially_copyable<volatile _Tp>
    {
      enum { __value = 0 };
    };


  template<typename _OutputIter, typename _InputIter>
    struct __memcpyable
    {
      enum { __value = 0 };
    };


  template<typename _Tp>
    struct __memcpyable<_Tp*, _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };


  template<typename _Tp>
    struct __memcpyable<_Tp*, const _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };

  template<typename _Tp> struct __memcpyable_integer;




  template<typename _Tp, typename _Up>
    struct __memcpyable<_Tp*, _Up*>
    {
      enum {
 __value = __memcpyable_integer<_Tp>::__width != 0
      && ((int)__memcpyable_integer<_Tp>::__width
     == (int)__memcpyable_integer<_Up>::__width)
      };
    };


  template<typename _Tp, typename _Up>
    struct __memcpyable<_Tp*, const _Up*>
    : __memcpyable<_Tp*, _Up*>
    { };

  template<typename _Tp>
    struct __memcpyable_integer
    {
      enum {
 __width = __is_integer<_Tp>::__value ? (sizeof(_Tp) * 8) : 0
      };
    };


  template<typename _Tp>
    struct __memcpyable_integer<volatile _Tp>
    { enum { __width = 0 }; };
# 551 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  __extension__
  template<>
    struct __memcpyable_integer<__int128> { enum { __width = 128 }; };
  __extension__
  template<>
    struct __memcpyable_integer<unsigned __int128> { enum { __width = 128 }; };
# 592 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Iter1, typename _Iter2>
    struct __memcmpable
    {
      enum { __value = 0 };
    };


  template<typename _Tp>
    struct __memcmpable<_Tp*, _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };

  template<typename _Tp>
    struct __memcmpable<const _Tp*, _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };

  template<typename _Tp>
    struct __memcmpable<_Tp*, const _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };







  template<typename _Tp, bool _TreatAsBytes =



 __is_byte<_Tp>::__value

    >
    struct __is_memcmp_ordered
    {
      static const bool __value = _Tp(-1) > _Tp(1);
    };

  template<typename _Tp>
    struct __is_memcmp_ordered<_Tp, false>
    {
      static const bool __value = false;
    };


  template<typename _Tp, typename _Up, bool = sizeof(_Tp) == sizeof(_Up)>
    struct __is_memcmp_ordered_with
    {
      static const bool __value = __is_memcmp_ordered<_Tp>::__value
 && __is_memcmp_ordered<_Up>::__value;
    };

  template<typename _Tp, typename _Up>
    struct __is_memcmp_ordered_with<_Tp, _Up, false>
    {
      static const bool __value = false;
    };
# 686 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Tp>
    struct __is_move_iterator
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };



  template<typename _Iterator>
   
    inline _Iterator
    __miter_base(_Iterator __it)
    { return __it; }


}
}

#pragma GCC diagnostic pop
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/type_traits.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/type_traits.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"

extern "C++" {

namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{



  template<bool, typename>
    struct __enable_if
    { };

  template<typename _Tp>
    struct __enable_if<true, _Tp>
    { typedef _Tp __type; };



  template<bool _Cond, typename _Iftrue, typename _Iffalse>
    struct __conditional_type
    { typedef _Iftrue __type; };

  template<typename _Iftrue, typename _Iffalse>
    struct __conditional_type<false, _Iftrue, _Iffalse>
    { typedef _Iffalse __type; };



  template<typename _Tp>
    struct __add_unsigned
    {
    private:
      typedef __enable_if<std::__is_integer<_Tp>::__value, _Tp> __if_type;

    public:
      typedef typename __if_type::__type __type;
    };

  template<>
    struct __add_unsigned<char>
    { typedef unsigned char __type; };

  template<>
    struct __add_unsigned<signed char>
    { typedef unsigned char __type; };

  template<>
    struct __add_unsigned<short>
    { typedef unsigned short __type; };

  template<>
    struct __add_unsigned<int>
    { typedef unsigned int __type; };

  template<>
    struct __add_unsigned<long>
    { typedef unsigned long __type; };

  template<>
    struct __add_unsigned<long long>
    { typedef unsigned long long __type; };


  template<>
    struct __add_unsigned<bool>;

  template<>
    struct __add_unsigned<wchar_t>;



  template<typename _Tp>
    struct __remove_unsigned
    {
    private:
      typedef __enable_if<std::__is_integer<_Tp>::__value, _Tp> __if_type;

    public:
      typedef typename __if_type::__type __type;
    };

  template<>
    struct __remove_unsigned<char>
    { typedef signed char __type; };

  template<>
    struct __remove_unsigned<unsigned char>
    { typedef signed char __type; };

  template<>
    struct __remove_unsigned<unsigned short>
    { typedef short __type; };

  template<>
    struct __remove_unsigned<unsigned int>
    { typedef int __type; };

  template<>
    struct __remove_unsigned<unsigned long>
    { typedef long __type; };

  template<>
    struct __remove_unsigned<unsigned long long>
    { typedef long long __type; };


  template<>
    struct __remove_unsigned<bool>;

  template<>
    struct __remove_unsigned<wchar_t>;



  template<typename _Type>
   
    inline bool
    __is_null_pointer(_Type* __ptr)
    { return __ptr == 0; }

  template<typename _Type>
   
    inline bool
    __is_null_pointer(_Type)
    { return false; }
# 175 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/type_traits.h" 3
  template<typename _Tp, bool = std::__is_integer<_Tp>::__value>
    struct __promote
    { typedef double __type; };




  template<typename _Tp>
    struct __promote<_Tp, false>
    { };

  template<>
    struct __promote<long double>
    { typedef long double __type; };

  template<>
    struct __promote<double>
    { typedef double __type; };

  template<>
    struct __promote<float>
    { typedef float __type; };
# 246 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/type_traits.h" 3
  template<typename _Tp, typename _Up,
           typename _Tp2 = typename __promote<_Tp>::__type,
           typename _Up2 = typename __promote<_Up>::__type>
    struct __promote_2
    {
      typedef __typeof__(_Tp2() + _Up2()) __type;
    };

  template<typename _Tp, typename _Up, typename _Vp,
           typename _Tp2 = typename __promote<_Tp>::__type,
           typename _Up2 = typename __promote<_Up>::__type,
           typename _Vp2 = typename __promote<_Vp>::__type>
    struct __promote_3
    {
      typedef __typeof__(_Tp2() + _Up2() + _Vp2()) __type;
    };

  template<typename _Tp, typename _Up, typename _Vp, typename _Wp,
           typename _Tp2 = typename __promote<_Tp>::__type,
           typename _Up2 = typename __promote<_Up>::__type,
           typename _Vp2 = typename __promote<_Vp>::__type,
           typename _Wp2 = typename __promote<_Wp>::__type>
    struct __promote_4
    {
      typedef __typeof__(_Tp2() + _Up2() + _Vp2() + _Wp2()) __type;
    };



}
}

#pragma GCC diagnostic pop
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 2 3

namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{

# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3
  template<typename _Tp>
    struct __is_integer_nonstrict
    : public std::__is_integer<_Tp>
    {
      using std::__is_integer<_Tp>::__value;


      enum { __width = __value ? sizeof(_Tp) * 8 : 0 };
    };

  template<typename _Value>
    struct __numeric_traits_integer
    {







      static const bool __is_signed = (_Value)(-1) < 0;
      static const int __digits
 = __is_integer_nonstrict<_Value>::__width - __is_signed;


      static const _Value __max = __is_signed
 ? (((((_Value)1 << (__digits - 1)) - 1) << 1) + 1)
 : ~(_Value)0;
      static const _Value __min = __is_signed ? -__max - 1 : (_Value)0;
    };

  template<typename _Value>
    const _Value __numeric_traits_integer<_Value>::__min;

  template<typename _Value>
    const _Value __numeric_traits_integer<_Value>::__max;

  template<typename _Value>
    const bool __numeric_traits_integer<_Value>::__is_signed;

  template<typename _Value>
    const int __numeric_traits_integer<_Value>::__digits;
# 132 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3
  __extension__ template<> struct __is_integer_nonstrict<__int128> { enum { __value = 1 }; typedef std::__true_type __type; enum { __width = 128 }; }; __extension__ template<> struct __is_integer_nonstrict<unsigned __int128> { enum { __value = 1 }; typedef std::__true_type __type; enum { __width = 128 }; };
# 159 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3
  template<typename _Value>
    struct __numeric_traits_floating
    {

      static const int __max_digits10 = (2 + (std::__are_same<_Value, float>::__value ? 24 : std::__are_same<_Value, double>::__value ? 53 : 64) * 643L / 2136);


      static const bool __is_signed = true;
      static const int __digits10 = (std::__are_same<_Value, float>::__value ? 6 : std::__are_same<_Value, double>::__value ? 15 : 18);
      static const int __max_exponent10 = (std::__are_same<_Value, float>::__value ? 38 : std::__are_same<_Value, double>::__value ? 308 : 4932);
    };

  template<typename _Value>
    const int __numeric_traits_floating<_Value>::__max_digits10;

  template<typename _Value>
    const bool __numeric_traits_floating<_Value>::__is_signed;

  template<typename _Value>
    const int __numeric_traits_floating<_Value>::__digits10;

  template<typename _Value>
    const int __numeric_traits_floating<_Value>::__max_exponent10;






  template<typename _Value>
    struct __numeric_traits
    : public __numeric_traits_integer<_Value>
    { };

  template<>
    struct __numeric_traits<float>
    : public __numeric_traits_floating<float>
    { };

  template<>
    struct __numeric_traits<double>
    : public __numeric_traits_floating<double>
    { };

  template<>
    struct __numeric_traits<long double>
    : public __numeric_traits_floating<long double>
    { };
# 240 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3

}
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 77 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  namespace __detail
  {


    template<typename _Tp>
      inline _Tp*
      __get_temporary_buffer(ptrdiff_t __len) throw()
      {
 if (__builtin_expect(size_t(__len) > (size_t(-1) / sizeof(_Tp)), 0))
   return 0;







 return (_Tp*) __builtin_operator_new(__len * sizeof(_Tp), nothrow_t());
      }



    template<typename _Tp>
      inline void
      __return_temporary_buffer(_Tp* __p,
    size_t __len __attribute__((__unused__)))
      {
# 118 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
 __builtin_operator_delete((__p));
      }

  }
# 140 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  template<typename _Tp>
   
    pair<_Tp*, ptrdiff_t>
    get_temporary_buffer(ptrdiff_t __len)
    {
      const ptrdiff_t __max =
 __gnu_cxx::__numeric_traits<ptrdiff_t>::__max / sizeof(_Tp);
      if (__len > __max)
 __len = __max;

      while (__len > 0)
 {
   if (_Tp* __tmp = __detail::__get_temporary_buffer<_Tp>(__len))
     return pair<_Tp*, ptrdiff_t>(__tmp, __len);
   __len = __len == 1 ? 0 : ((__len + 1) / 2);
 }
      return pair<_Tp*, ptrdiff_t>();
    }
# 166 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  template<typename _Tp>
   
    inline void
    return_temporary_buffer(_Tp* __p)
    {





      __builtin_operator_delete(__p);
    }
# 187 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  template<typename _ForwardIterator, typename _Tp>
    class _Temporary_buffer
    {

     

    public:
      typedef _Tp value_type;
      typedef value_type* pointer;
      typedef pointer iterator;
      typedef ptrdiff_t size_type;

    protected:
      size_type _M_original_len;
      struct _Impl
      {
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
 explicit
 _Impl(ptrdiff_t __original_len)
 {
   pair<pointer, size_type> __p(
     std::get_temporary_buffer<value_type>(__original_len));
   _M_len = __p.second;
   _M_buffer = __p.first;
 }
#pragma GCC diagnostic pop

 ~_Impl()
 { std::__detail::__return_temporary_buffer(_M_buffer, _M_len); }

 size_type _M_len;
 pointer _M_buffer;
      } _M_impl;

    public:

      size_type
      size() const
      { return _M_impl._M_len; }


      size_type
      requested_size() const
      { return _M_original_len; }


      iterator
      begin()
      { return _M_impl._M_buffer; }


      iterator
      end()
      { return _M_impl._M_buffer + _M_impl._M_len; }





      _Temporary_buffer(_ForwardIterator __seed, size_type __original_len);

      ~_Temporary_buffer()
      { std::_Destroy(_M_impl._M_buffer, _M_impl._M_buffer + _M_impl._M_len); }

    private:

      _Temporary_buffer(const _Temporary_buffer&);

      void
      operator=(const _Temporary_buffer&);
    };


  template<bool>
    struct __uninitialized_construct_buf_dispatch
    {
      template<typename _Pointer, typename _ForwardIterator>
        static void
        __ucr(_Pointer __first, _Pointer __last,
       _ForwardIterator __seed)
        {
   if (__builtin_expect(__first == __last, 0))
     return;

   _Pointer __cur = __first;
   try
     {
       std::_Construct(std::__addressof(*__first),
         (*__seed));
       _Pointer __prev = __cur;
       ++__cur;
       for(; __cur != __last; ++__cur, ++__prev)
  std::_Construct(std::__addressof(*__cur),
    (*__prev));
       *__seed = (*__prev);
     }
   catch(...)
     {
       std::_Destroy(__first, __cur);
       throw;
     }
 }
    };

  template<>
    struct __uninitialized_construct_buf_dispatch<true>
    {
      template<typename _Pointer, typename _ForwardIterator>
        static void
        __ucr(_Pointer, _Pointer, _ForwardIterator) { }
    };
# 311 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  template<typename _Tp, typename _ForwardIterator>
    inline void
    __uninitialized_construct_buf(_Tp* __first, _Tp* __last,
      _ForwardIterator __seed)
    {
      std::__uninitialized_construct_buf_dispatch<
 __has_trivial_constructor(_Tp)>::
   __ucr(__first, __last, __seed);
    }

  template<typename _ForwardIterator, typename _Tp>
    _Temporary_buffer<_ForwardIterator, _Tp>::
    _Temporary_buffer(_ForwardIterator __seed, size_type __original_len)
    : _M_original_len(__original_len), _M_impl(__original_len)
    {
      std::__uninitialized_construct_buf(begin(), end(), __seed);
    }


}
# 69 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 1 3
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 1 3
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ptr_traits.h" 1 3
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 2 3
# 85 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 108 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
 
# 108 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
# 130 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
    class reverse_iterator
    : public iterator<typename iterator_traits<_Iterator>::iterator_category,
        typename iterator_traits<_Iterator>::value_type,
        typename iterator_traits<_Iterator>::difference_type,
        typename iterator_traits<_Iterator>::pointer,
                      typename iterator_traits<_Iterator>::reference>
    {
      template<typename _Iter>
 friend class reverse_iterator;
# 149 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
    protected:
      _Iterator current;

      typedef iterator_traits<_Iterator> __traits_type;

    public:
      typedef _Iterator iterator_type;
      typedef typename __traits_type::pointer pointer;

      typedef typename __traits_type::difference_type difference_type;
      typedef typename __traits_type::reference reference;
# 180 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      reverse_iterator()
     
      : current()
      { }




      explicit
      reverse_iterator(iterator_type __x)
     
      : current(__x)
      { }




     
      reverse_iterator(const reverse_iterator& __x)
     
      : current(__x.current)
      { }
# 212 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      template<typename _Iter>




        reverse_iterator(const reverse_iterator<_Iter>& __x)

 : current(__x.current)
 { }
# 241 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      iterator_type
      base() const
     
      { return current; }
# 257 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      reference
      operator*() const
      {
 _Iterator __tmp = current;
 return *--__tmp;
      }






     
      pointer
      operator->() const




      {


 _Iterator __tmp = current;
 --__tmp;
 return _S_to_pointer(__tmp);
      }






      reverse_iterator&
      operator++()
      {
 --current;
 return *this;
      }






      reverse_iterator
      operator++(int)
      {
 reverse_iterator __tmp = *this;
 --current;
 return __tmp;
      }






      reverse_iterator&
      operator--()
      {
 ++current;
 return *this;
      }






      reverse_iterator
      operator--(int)
      {
 reverse_iterator __tmp = *this;
 ++current;
 return __tmp;
      }






     
      reverse_iterator
      operator+(difference_type __n) const
      { return reverse_iterator(current - __n); }







      reverse_iterator&
      operator+=(difference_type __n)
      {
 current -= __n;
 return *this;
      }






     
      reverse_iterator
      operator-(difference_type __n) const
      { return reverse_iterator(current + __n); }







      reverse_iterator&
      operator-=(difference_type __n)
      {
 current += __n;
 return *this;
      }






     
      reference
      operator[](difference_type __n) const
      { return *(*this + __n); }
# 417 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
    private:
      template<typename _Tp>
 static _Tp*
 _S_to_pointer(_Tp* __p)
        { return __p; }

      template<typename _Tp>
 static pointer
 _S_to_pointer(_Tp __t)
        { return __t.operator->(); }
    };
# 440 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
   
    inline bool
    operator==(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return __x.base() == __y.base(); }

  template<typename _Iterator>
   
    inline bool
    operator<(const reverse_iterator<_Iterator>& __x,
       const reverse_iterator<_Iterator>& __y)
    { return __y.base() < __x.base(); }

  template<typename _Iterator>
   
    inline bool
    operator!=(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return !(__x == __y); }

  template<typename _Iterator>
   
    inline bool
    operator>(const reverse_iterator<_Iterator>& __x,
       const reverse_iterator<_Iterator>& __y)
    { return __y < __x; }

  template<typename _Iterator>
   
    inline bool
    operator<=(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return !(__y < __x); }

  template<typename _Iterator>
   
    inline bool
    operator>=(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return !(__x < __y); }




  template<typename _IteratorL, typename _IteratorR>
   
    inline bool
    operator==(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() == __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
   
    inline bool
    operator<(const reverse_iterator<_IteratorL>& __x,
       const reverse_iterator<_IteratorR>& __y)
    { return __x.base() > __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
   
    inline bool
    operator!=(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() != __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
   
    inline bool
    operator>(const reverse_iterator<_IteratorL>& __x,
       const reverse_iterator<_IteratorR>& __y)
    { return __x.base() < __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    inline bool
    operator<=(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() >= __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
   
    inline bool
    operator>=(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() <= __y.base(); }
# 603 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
    inline typename reverse_iterator<_Iterator>::difference_type
    operator-(const reverse_iterator<_Iterator>& __x,
       const reverse_iterator<_Iterator>& __y)
    { return __y.base() - __x.base(); }

  template<typename _IteratorL, typename _IteratorR>
    inline typename reverse_iterator<_IteratorL>::difference_type
    operator-(const reverse_iterator<_IteratorL>& __x,
       const reverse_iterator<_IteratorR>& __y)
    { return __y.base() - __x.base(); }
# 626 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
   
    inline reverse_iterator<_Iterator>
    operator+(typename reverse_iterator<_Iterator>::difference_type __n,
       const reverse_iterator<_Iterator>& __x)
    { return reverse_iterator<_Iterator>(__x.base() - __n); }
# 676 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    class back_insert_iterator
    : public iterator<output_iterator_tag, void, void, void, void>
    {
    protected:
      _Container* container;

    public:

      typedef _Container container_type;





      explicit
      back_insert_iterator(_Container& __x)
      : container(std::__addressof(__x)) { }
# 707 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      back_insert_iterator&
      operator=(typename _Container::const_reference __value)
      {
 container->push_back(__value);
 return *this;
      }
# 732 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      back_insert_iterator&
      operator*()
      { return *this; }


     
      back_insert_iterator&
      operator++()
      { return *this; }


     
      back_insert_iterator
      operator++(int)
      { return *this; }
    };
# 761 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
   
    inline back_insert_iterator<_Container>
    back_inserter(_Container& __x)
    { return back_insert_iterator<_Container>(__x); }
# 777 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    class front_insert_iterator
    : public iterator<output_iterator_tag, void, void, void, void>
    {
    protected:
      _Container* container;

    public:

      typedef _Container container_type;





      explicit
      front_insert_iterator(_Container& __x)
      : container(std::__addressof(__x)) { }
# 808 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      front_insert_iterator&
      operator=(typename _Container::const_reference __value)
      {
 container->push_front(__value);
 return *this;
      }
# 833 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      front_insert_iterator&
      operator*()
      { return *this; }


     
      front_insert_iterator&
      operator++()
      { return *this; }


     
      front_insert_iterator
      operator++(int)
      { return *this; }
    };
# 862 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
   
    inline front_insert_iterator<_Container>
    front_inserter(_Container& __x)
    { return front_insert_iterator<_Container>(__x); }
# 882 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    class insert_iterator
    : public iterator<output_iterator_tag, void, void, void, void>
    {



      typedef typename _Container::iterator _Iter;

    protected:
      _Container* container;
      _Iter iter;

    public:

      typedef _Container container_type;
# 907 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      insert_iterator(_Container& __x, _Iter __i)
      : container(std::__addressof(__x)), iter(__i) {}
# 935 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      insert_iterator&
      operator=(typename _Container::const_reference __value)
      {
 iter = container->insert(iter, __value);
 ++iter;
 return *this;
      }
# 963 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      insert_iterator&
      operator*()
      { return *this; }


     
      insert_iterator&
      operator++()
      { return *this; }


     
      insert_iterator&
      operator++(int)
      { return *this; }
    };

#pragma GCC diagnostic pop
# 1002 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
   
    inline insert_iterator<_Container>
    inserter(_Container& __x, typename _Container::iterator __i)
    { return insert_iterator<_Container>(__x, __i); }





}

namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{

# 1025 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator, typename _Container>
    class __normal_iterator
    {
    protected:
      _Iterator _M_current;

      typedef std::iterator_traits<_Iterator> __traits_type;







    public:
      typedef _Iterator iterator_type;
      typedef typename __traits_type::iterator_category iterator_category;
      typedef typename __traits_type::value_type value_type;
      typedef typename __traits_type::difference_type difference_type;
      typedef typename __traits_type::reference reference;
      typedef typename __traits_type::pointer pointer;





      __attribute__((__always_inline__))
     
      __normal_iterator()
      : _M_current() { }

      __attribute__((__always_inline__))
      explicit
      __normal_iterator(const _Iterator& __i)
      : _M_current(__i) { }
# 1075 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      template<typename _Iter>
 __attribute__((__always_inline__))
 __normal_iterator(const __normal_iterator<_Iter,
     typename __enable_if<
        (std::__are_same<_Iter, typename _Container::pointer>::__value),
        _Container>::__type>& __i)

        : _M_current(__i.base()) { }



      __attribute__((__always_inline__))
     
      reference
      operator*() const
      { return *_M_current; }

      __attribute__((__always_inline__))
     
      pointer
      operator->() const
      { return _M_current; }

      __attribute__((__always_inline__))
     
      __normal_iterator&
      operator++()
      {
 ++_M_current;
 return *this;
      }

      __attribute__((__always_inline__))
     
      __normal_iterator
      operator++(int)
      { return __normal_iterator(_M_current++); }



      __attribute__((__always_inline__))
     
      __normal_iterator&
      operator--()
      {
 --_M_current;
 return *this;
      }

      __attribute__((__always_inline__))
     
      __normal_iterator
      operator--(int)
      { return __normal_iterator(_M_current--); }



      __attribute__((__always_inline__))
     
      reference
      operator[](difference_type __n) const
      { return _M_current[__n]; }

      __attribute__((__always_inline__))
     
      __normal_iterator&
      operator+=(difference_type __n)
      { _M_current += __n; return *this; }

      __attribute__((__always_inline__))
     
      __normal_iterator
      operator+(difference_type __n) const
      { return __normal_iterator(_M_current + __n); }

      __attribute__((__always_inline__))
     
      __normal_iterator&
      operator-=(difference_type __n)
      { _M_current -= __n; return *this; }

      __attribute__((__always_inline__))
     
      __normal_iterator
      operator-(difference_type __n) const
      { return __normal_iterator(_M_current - __n); }

      __attribute__((__always_inline__))
     
      const _Iterator&
      base() const
      { return _M_current; }
    };
# 1217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _IteratorL, typename _IteratorR, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator==(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
   
    { return __lhs.base() == __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator==(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
   
    { return __lhs.base() == __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator!=(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
   
    { return __lhs.base() != __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator!=(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
   
    { return __lhs.base() != __rhs.base(); }


  template<typename _IteratorL, typename _IteratorR, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator<(const __normal_iterator<_IteratorL, _Container>& __lhs,
       const __normal_iterator<_IteratorR, _Container>& __rhs)
   
    { return __lhs.base() < __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator<(const __normal_iterator<_Iterator, _Container>& __lhs,
       const __normal_iterator<_Iterator, _Container>& __rhs)
   
    { return __lhs.base() < __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator>(const __normal_iterator<_IteratorL, _Container>& __lhs,
       const __normal_iterator<_IteratorR, _Container>& __rhs)
   
    { return __lhs.base() > __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator>(const __normal_iterator<_Iterator, _Container>& __lhs,
       const __normal_iterator<_Iterator, _Container>& __rhs)
   
    { return __lhs.base() > __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator<=(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
   
    { return __lhs.base() <= __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator<=(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
   
    { return __lhs.base() <= __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator>=(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
   
    { return __lhs.base() >= __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator>=(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
   
    { return __lhs.base() >= __rhs.base(); }






  template<typename _IteratorL, typename _IteratorR, typename _Container>
# 1328 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
    inline typename __normal_iterator<_IteratorL, _Container>::difference_type
    operator-(const __normal_iterator<_IteratorL, _Container>& __lhs,
       const __normal_iterator<_IteratorR, _Container>& __rhs)

    { return __lhs.base() - __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline typename __normal_iterator<_Iterator, _Container>::difference_type
    operator-(const __normal_iterator<_Iterator, _Container>& __lhs,
       const __normal_iterator<_Iterator, _Container>& __rhs)
   
    { return __lhs.base() - __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline __normal_iterator<_Iterator, _Container>
    operator+(typename __normal_iterator<_Iterator, _Container>::difference_type
       __n, const __normal_iterator<_Iterator, _Container>& __i)
   
    { return __normal_iterator<_Iterator, _Container>(__i.base() + __n); }


}

namespace std __attribute__ ((__visibility__ ("default")))
{

# 2980 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3

}

namespace __gnu_debug
{
  template<typename _Iterator, typename _Sequence, typename _Category>
    class _Safe_iterator;
}

namespace std __attribute__ ((__visibility__ ("default")))
{





  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
   
    inline _Iterator
    __niter_base(__gnu_cxx::__normal_iterator<_Iterator, _Container> __it)
   
    { return __it.base(); }


  template<typename _Iterator>
    __attribute__((__always_inline__))
   
    inline _Iterator
    __niter_base(_Iterator __it)
   
    { return __it; }




  template<typename _Ite, typename _Seq>
    _Ite
    __niter_base(const ::__gnu_debug::_Safe_iterator<_Ite, _Seq,
   std::random_access_iterator_tag>&);

 template<typename _Ite, typename _Cont, typename _Seq>
    _Ite
    __niter_base(const ::__gnu_debug::_Safe_iterator<
   ::__gnu_cxx::__normal_iterator<_Ite, _Cont>, _Seq,
   std::random_access_iterator_tag>&);
# 3069 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _From, typename _To>
   
   
    inline _From
    __niter_wrap(_From __from, _To __res)
    { return __from + (std::__niter_base(__res) - std::__niter_base(__from)); }


  template<typename _Iterator>
    __attribute__((__always_inline__))
   
    inline _Iterator
    __niter_wrap(const _Iterator&, _Iterator __res)
    { return __res; }
# 3113 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3

}
# 69 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 1 3
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 970 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
  template<typename _Alloc, bool = __is_empty(_Alloc)>
    struct __alloc_swap
    { static void _S_do_it(_Alloc&, _Alloc&) { } };

  template<typename _Alloc>
    struct __alloc_swap<_Alloc, false>
    {
      static void
      _S_do_it(_Alloc& __one, _Alloc& __two)
      {

 if (__one != __two)
   swap(__one, __two);
      }
    };
# 1023 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
  template<typename _ForwardIterator, typename _Allocator>
   
    void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last,
      _Allocator& __alloc)
    {
      for (; __first != __last; ++__first)

 __alloc.destroy(std::__addressof(*__first));




    }


  template<typename _ForwardIterator, typename _Tp>
    __attribute__((__always_inline__))
    inline void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last,
      allocator<_Tp>&)
    {
      std::_Destroy(__first, __last);
    }





}
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 2 3

namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{






template<typename _Alloc, typename = typename _Alloc::value_type>
  struct __alloc_traits



  {
    typedef _Alloc allocator_type;
# 131 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 3
    typedef typename _Alloc::pointer pointer;
    typedef typename _Alloc::const_pointer const_pointer;
    typedef typename _Alloc::value_type value_type;
    typedef typename _Alloc::reference reference;
    typedef typename _Alloc::const_reference const_reference;
    typedef typename _Alloc::size_type size_type;
    typedef typename _Alloc::difference_type difference_type;

    __attribute__((__always_inline__))
    static pointer
    allocate(_Alloc& __a, size_type __n)
    { return __a.allocate(__n); }

    template<typename _Hint>
      __attribute__((__always_inline__))
      static pointer
      allocate(_Alloc& __a, size_type __n, _Hint __hint)
      { return __a.allocate(__n, __hint); }

    __attribute__((__always_inline__))
    static void deallocate(_Alloc& __a, pointer __p, size_type __n)
    { __a.deallocate(__p, __n); }

    template<typename _Tp>
      __attribute__((__always_inline__))
      static void construct(_Alloc& __a, pointer __p, const _Tp& __arg)
      { __a.construct(__p, __arg); }

    __attribute__((__always_inline__))
    static void destroy(_Alloc& __a, pointer __p)
    { __a.destroy(__p); }

    __attribute__((__always_inline__))
    static size_type max_size(const _Alloc& __a)
    { return __a.max_size(); }

    __attribute__((__always_inline__))
    static const _Alloc& _S_select_on_copy(const _Alloc& __a) { return __a; }

    __attribute__((__always_inline__))
    static void _S_on_swap(_Alloc& __a, _Alloc& __b)
    {


      std::__alloc_swap<_Alloc>::_S_do_it(__a, __b);
    }

    template<typename _Tp>
      struct rebind
      { typedef typename _Alloc::template rebind<_Tp>::other other; };

  };


}
# 70 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{








  template<typename _ForwardIterator, typename _Alloc = void>
    struct _UninitDestroyGuard
    {
     
      explicit
      _UninitDestroyGuard(_ForwardIterator& __first, _Alloc& __a)
      : _M_first(__first), _M_cur(__builtin_addressof(__first)), _M_alloc(__a)
      { }

     
      ~_UninitDestroyGuard()
      {
 if (__builtin_expect(_M_cur != 0, 0))
   std::_Destroy(_M_first, *_M_cur, _M_alloc);
      }

     
      void release() { _M_cur = 0; }

    private:
      _ForwardIterator const _M_first;
      _ForwardIterator* _M_cur;
      _Alloc& _M_alloc;

      _UninitDestroyGuard(const _UninitDestroyGuard&);
    };

  template<typename _ForwardIterator>
    struct _UninitDestroyGuard<_ForwardIterator, void>
    {
     
      explicit
      _UninitDestroyGuard(_ForwardIterator& __first)
      : _M_first(__first), _M_cur(__builtin_addressof(__first))
      { }

     
      ~_UninitDestroyGuard()
      {
 if (__builtin_expect(_M_cur != 0, 0))
   std::_Destroy(_M_first, *_M_cur);
      }

     
      void release() { _M_cur = 0; }

      _ForwardIterator const _M_first;
      _ForwardIterator* _M_cur;

    private:
      _UninitDestroyGuard(const _UninitDestroyGuard&);
    };



  template<typename _InputIterator, typename _Sentinel,
    typename _ForwardIterator>
   
    _ForwardIterator
    __do_uninit_copy(_InputIterator __first, _Sentinel __last,
       _ForwardIterator __result)
    {
      _UninitDestroyGuard<_ForwardIterator> __guard(__result);
      for (; __first != __last; ++__first, (void)++__result)
 std::_Construct(std::__addressof(*__result), *__first);
      __guard.release();
      return __result;
    }




  template<typename _Iter,
    typename _Base = __decltype(std::__niter_base(*(_Iter*)0))>
    struct __unwrappable_niter
    { enum { __value = false }; };

  template<typename _Iter, typename _Tp>
    struct __unwrappable_niter<_Iter, _Tp*>
    { enum { __value = true }; };


  template<bool _CanMemcpy>
    struct __uninitialized_copy
    {
      template<typename _InputIterator, typename _ForwardIterator>
        static _ForwardIterator
        __uninit_copy(_InputIterator __first, _InputIterator __last,
        _ForwardIterator __result)
 { return std::__do_uninit_copy(__first, __last, __result); }
    };

  template<>
    struct __uninitialized_copy<true>
    {

      template<typename _InputIterator, typename _ForwardIterator>
        static _ForwardIterator
        __uninit_copy(_InputIterator __first, _InputIterator __last,
        _ForwardIterator __result)
 {
   if (__unwrappable_niter<_InputIterator>::__value
  && __unwrappable_niter<_ForwardIterator>::__value)
     {
       __uninit_copy(std::__niter_base(__first),
       std::__niter_base(__last),
       std::__niter_base(__result));
       std::advance(__result, std::distance(__first, __last));
       return __result;
     }
   else
     return std::__do_uninit_copy(__first, __last, __result);
 }


      template<typename _Tp, typename _Up>
 static _Up*
 __uninit_copy(_Tp* __first, _Tp* __last, _Up* __result)
 {


   typedef __typeof__(static_cast<_Up>(*__first)) __check
     __attribute__((__unused__));

   const ptrdiff_t __n = __last - __first;
   if (__builtin_expect(__n > 0, true))
     {
       __builtin_memcpy(__result, __first, __n * sizeof(_Tp));
       __result += __n;
     }
   return __result;
 }
    };



#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
# 228 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _InputIterator, typename _ForwardIterator>
    inline _ForwardIterator
    uninitialized_copy(_InputIterator __first, _InputIterator __last,
         _ForwardIterator __result)
    {
# 299 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      typedef typename iterator_traits<_InputIterator>::value_type
 _ValueType1;
      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType2;

      const bool __can_memcpy
 = __memcpyable<_ValueType1*, _ValueType2*>::__value
     && __is_trivially_constructible(_ValueType2, __decltype(*__first));

      return __uninitialized_copy<__can_memcpy>::
        __uninit_copy(__first, __last, __result);

    }
#pragma GCC diagnostic pop




  template<typename _ForwardIterator, typename _Tp>
    void
    __do_uninit_fill(_ForwardIterator __first, _ForwardIterator __last,
       const _Tp& __x)
    {
      _UninitDestroyGuard<_ForwardIterator> __guard(__first);
      for (; __first != __last; ++__first)
 std::_Construct(std::__addressof(*__first), __x);
      __guard.release();
    }



  template<bool _CanMemset>
    struct __uninitialized_fill
    {
      template<typename _ForwardIterator, typename _Tp>
 static void
 __uninit_fill(_ForwardIterator __first, _ForwardIterator __last,
        const _Tp& __x)
 { std::__do_uninit_fill(__first, __last, __x); }
    };

  template<>
    struct __uninitialized_fill<true>
    {

      template<typename _ForwardIterator, typename _Tp>
 static void
 __uninit_fill(_ForwardIterator __first, _ForwardIterator __last,
        const _Tp& __x)
 {
   if (__unwrappable_niter<_ForwardIterator>::__value)
     __uninit_fill(std::__niter_base(__first),
     std::__niter_base(__last),
     __x);
   else
     std::__do_uninit_copy(__first, __last, __x);
 }


      template<typename _Up, typename _Tp>
 static void
 __uninit_fill(_Up* __first, _Up* __last, const _Tp& __x)
 {


   typedef __typeof__(static_cast<_Up>(__x)) __check
     __attribute__((__unused__));

   if (__first != __last)
     __builtin_memset(__first, (unsigned char)__x, __last - __first);
 }
    };
# 383 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _ForwardIterator, typename _Tp>
    inline void
    uninitialized_fill(_ForwardIterator __first, _ForwardIterator __last,
         const _Tp& __x)
    {
# 397 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;
# 430 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      const bool __can_memset = __is_byte<_ValueType>::__value
      && __is_integer<_Tp>::__value;

      __uninitialized_fill<__can_memset>::__uninit_fill(__first, __last, __x);

    }




  template<typename _ForwardIterator, typename _Size, typename _Tp>
   
    _ForwardIterator
    __do_uninit_fill_n(_ForwardIterator __first, _Size __n, const _Tp& __x)
    {
      _UninitDestroyGuard<_ForwardIterator> __guard(__first);
# 457 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      for (; __n--; ++__first)
 std::_Construct(std::__addressof(*__first), __x);
      __guard.release();
      return __first;
    }



  template<bool _CanMemset>
    struct __uninitialized_fill_n
    {
      template<typename _ForwardIterator, typename _Size, typename _Tp>
 static _ForwardIterator
        __uninit_fill_n(_ForwardIterator __first, _Size __n,
   const _Tp& __x)
 { return std::__do_uninit_fill_n(__first, __n, __x); }
    };

  template<>
    struct __uninitialized_fill_n<true>
    {

      template<typename _ForwardIterator, typename _Size, typename _Tp>
 static _ForwardIterator
        __uninit_fill_n(_ForwardIterator __first, _Size __n,
   const _Tp& __x)
 {
   if (__unwrappable_niter<_ForwardIterator>::__value)
     {
       _ForwardIterator __last = __first;
       std::advance(__last, __n);
       __uninitialized_fill<true>::__uninit_fill(__first, __last, __x);
       return __last;
     }
   else
     return std::__do_uninit_fill_n(__first, __n, __x);
 }
    };



#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
# 511 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _ForwardIterator, typename _Size, typename _Tp>
    inline _ForwardIterator
    uninitialized_fill_n(_ForwardIterator __first, _Size __n, const _Tp& __x)
    {






      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;
# 555 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      const bool __can_memset = __is_byte<_ValueType>::__value
      && __is_integer<_Tp>::__value
      && __is_integer<_Size>::__value;

      return __uninitialized_fill_n<__can_memset>::
 __uninit_fill_n(__first, __n, __x);

    }
#pragma GCC diagnostic pop
# 573 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _InputIterator, typename _Sentinel,
    typename _ForwardIterator, typename _Allocator>
   
    _ForwardIterator
    __uninitialized_copy_a(_InputIterator __first, _Sentinel __last,
      _ForwardIterator __result, _Allocator& __alloc)
    {
      _UninitDestroyGuard<_ForwardIterator, _Allocator>
 __guard(__result, __alloc);

      typedef __gnu_cxx::__alloc_traits<_Allocator> __traits;
      for (; __first != __last; ++__first, (void)++__result)
 __traits::construct(__alloc, std::__addressof(*__result), *__first);
      __guard.release();
      return __result;
    }


  template<typename _InputIterator, typename _Sentinel,
    typename _ForwardIterator, typename _Tp>
   
    inline _ForwardIterator
    __uninitialized_copy_a(_InputIterator __first, _Sentinel __last,
      _ForwardIterator __result, allocator<_Tp>&)
    {
# 619 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      return std::uninitialized_copy(__first, __last, __result);

    }


  template<typename _InputIterator, typename _ForwardIterator,
    typename _Allocator>
   
    inline _ForwardIterator
    __uninitialized_move_a(_InputIterator __first, _InputIterator __last,
      _ForwardIterator __result, _Allocator& __alloc)
    {
      return std::__uninitialized_copy_a((__first),
      (__last),
      __result, __alloc);
    }

  template<typename _InputIterator, typename _ForwardIterator,
    typename _Allocator>
   
    inline _ForwardIterator
    __uninitialized_move_if_noexcept_a(_InputIterator __first,
           _InputIterator __last,
           _ForwardIterator __result,
           _Allocator& __alloc)
    {
      return std::__uninitialized_copy_a
 ((__first),
  (__last), __result, __alloc);
    }

  template<typename _ForwardIterator, typename _Tp, typename _Allocator>
   
    void
    __uninitialized_fill_a(_ForwardIterator __first, _ForwardIterator __last,
      const _Tp& __x, _Allocator& __alloc)
    {
      _UninitDestroyGuard<_ForwardIterator, _Allocator>
 __guard(__first, __alloc);

      typedef __gnu_cxx::__alloc_traits<_Allocator> __traits;
      for (; __first != __last; ++__first)
 __traits::construct(__alloc, std::__addressof(*__first), __x);

      __guard.release();
    }


  template<typename _ForwardIterator, typename _Tp, typename _Tp2>
   
    inline void
    __uninitialized_fill_a(_ForwardIterator __first, _ForwardIterator __last,
      const _Tp& __x, allocator<_Tp2>&)
    {




      std::uninitialized_fill(__first, __last, __x);
    }


  template<typename _ForwardIterator, typename _Size, typename _Tp,
    typename _Allocator>
    
    _ForwardIterator
    __uninitialized_fill_n_a(_ForwardIterator __first, _Size __n,
        const _Tp& __x, _Allocator& __alloc)
    {
      _UninitDestroyGuard<_ForwardIterator, _Allocator>
 __guard(__first, __alloc);
      typedef __gnu_cxx::__alloc_traits<_Allocator> __traits;
      for (; __n > 0; --__n, (void) ++__first)
 __traits::construct(__alloc, std::__addressof(*__first), __x);
      __guard.release();
      return __first;
    }


  template<typename _ForwardIterator, typename _Size, typename _Tp,
    typename _Tp2>
   
    inline _ForwardIterator
    __uninitialized_fill_n_a(_ForwardIterator __first, _Size __n,
        const _Tp& __x, allocator<_Tp2>&)
    {




      return std::uninitialized_fill_n(__first, __n, __x);
    }
# 722 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _ForwardIterator, typename _Allocator>
    inline _ForwardIterator
    __uninitialized_copy_move(_InputIterator1 __first1,
         _InputIterator1 __last1,
         _InputIterator2 __first2,
         _InputIterator2 __last2,
         _ForwardIterator __result,
         _Allocator& __alloc)
    {
      _ForwardIterator __mid = std::__uninitialized_copy_a(__first1, __last1,
          __result, __alloc);
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__result,
        __alloc);
      __result = __mid;
      __result = std::__uninitialized_move_a(__first2, __last2, __mid, __alloc);
      __guard.release();
      return __result;
    }





  template<typename _InputIterator1, typename _InputIterator2,
    typename _ForwardIterator, typename _Allocator>
    inline _ForwardIterator
    __uninitialized_move_copy(_InputIterator1 __first1,
         _InputIterator1 __last1,
         _InputIterator2 __first2,
         _InputIterator2 __last2,
         _ForwardIterator __result,
         _Allocator& __alloc)
    {
      _ForwardIterator __mid = std::__uninitialized_move_a(__first1, __last1,
          __result, __alloc);
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__result,
        __alloc);
      __result = __mid;
      __result = std::__uninitialized_copy_a(__first2, __last2, __mid, __alloc);
      __guard.release();
      return __result;
    }




  template<typename _ForwardIterator, typename _Tp, typename _InputIterator,
    typename _Allocator>
    inline _ForwardIterator
    __uninitialized_fill_move(_ForwardIterator __result, _ForwardIterator __mid,
         const _Tp& __x, _InputIterator __first,
         _InputIterator __last, _Allocator& __alloc)
    {
      std::__uninitialized_fill_a(__result, __mid, __x, __alloc);
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__result,
        __alloc);
      __result = __mid;
      __result = std::__uninitialized_move_a(__first, __last, __mid, __alloc);
      __guard.release();
      return __result;
    }




  template<typename _InputIterator, typename _ForwardIterator, typename _Tp,
    typename _Allocator>
    inline void
    __uninitialized_move_fill(_InputIterator __first1, _InputIterator __last1,
         _ForwardIterator __first2,
         _ForwardIterator __last2, const _Tp& __x,
         _Allocator& __alloc)
    {
      _ForwardIterator __mid2 = std::__uninitialized_move_a(__first1, __last1,
           __first2,
           __alloc);
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__first2,
        __alloc);
      __first2 = __mid2;
      std::__uninitialized_fill_a(__mid2, __last2, __x, __alloc);
      __guard.release();
    }
# 1324 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3

}
# 72 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_raw_storage_iter.h" 1 3
# 59 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_raw_storage_iter.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{



 
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_raw_storage_iter.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"





  template <class _OutputIterator, class _Tp>
    class raw_storage_iterator
    : public iterator<output_iterator_tag, void, void, void, void>
    {
    protected:
      _OutputIterator _M_iter;

    public:
      explicit
      raw_storage_iterator(_OutputIterator __x)
      : _M_iter(__x) {}

      raw_storage_iterator&
      operator*() { return *this; }

      raw_storage_iterator&
      operator=(const _Tp& __element)
      {
 std::_Construct(std::__addressof(*_M_iter), __element);
 return *this;
      }
# 104 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_raw_storage_iter.h" 3
      raw_storage_iterator&
      operator++()
      {
 ++_M_iter;
 return *this;
      }

      raw_storage_iterator
      operator++(int)
      {
 raw_storage_iterator __tmp = *this;
 ++_M_iter;
 return __tmp;
      }



      _OutputIterator base() const { return _M_iter; }
    };
#pragma GCC diagnostic pop


}
# 73 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3
# 88 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 1 3
# 34 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/debug/debug.h" 1 3
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/debug/debug.h" 3
namespace std
{
  namespace __debug { }
}




namespace __gnu_debug
{
  using namespace std::__debug;

  template<typename _Ite, typename _Seq, typename _Cat>
    struct _Safe_iterator;
}
# 35 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
  template<typename _Tp1>
    struct auto_ptr_ref
    {
      _Tp1* _M_ptr;

      explicit
      auto_ptr_ref(_Tp1* __p): _M_ptr(__p) { }
    } ;

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
# 92 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
  template<typename _Tp>
    class auto_ptr
    {
    private:
      _Tp* _M_ptr;

    public:

      typedef _Tp element_type;







      explicit
      auto_ptr(element_type* __p = 0) throw() : _M_ptr(__p) { }
# 118 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      auto_ptr(auto_ptr& __a) throw() : _M_ptr(__a.release()) { }
# 130 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      template<typename _Tp1>
        auto_ptr(auto_ptr<_Tp1>& __a) throw() : _M_ptr(__a.release()) { }
# 141 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      auto_ptr&
      operator=(auto_ptr& __a) throw()
      {
 reset(__a.release());
 return *this;
      }
# 158 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      template<typename _Tp1>
        auto_ptr&
        operator=(auto_ptr<_Tp1>& __a) throw()
        {
   reset(__a.release());
   return *this;
 }
# 176 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      ~auto_ptr() { delete _M_ptr; }
# 186 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      element_type&
      operator*() const throw()
      {
 do { if (__builtin_expect(!bool(_M_ptr != 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h", 189, __PRETTY_FUNCTION__, "_M_ptr != 0"); } while (false);
 return *_M_ptr;
      }







      element_type*
      operator->() const throw()
      {
 do { if (__builtin_expect(!bool(_M_ptr != 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h", 202, __PRETTY_FUNCTION__, "_M_ptr != 0"); } while (false);
 return _M_ptr;
      }
# 216 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      element_type*
      get() const throw() { return _M_ptr; }
# 230 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      element_type*
      release() throw()
      {
 element_type* __tmp = _M_ptr;
 _M_ptr = 0;
 return __tmp;
      }
# 245 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      void
      reset(element_type* __p = 0) throw()
      {
 if (__p != _M_ptr)
   {
     delete _M_ptr;
     _M_ptr = __p;
   }
      }
# 270 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
      auto_ptr(auto_ptr_ref<element_type> __ref) throw()
      : _M_ptr(__ref._M_ptr) { }

      auto_ptr&
      operator=(auto_ptr_ref<element_type> __ref) throw()
      {
 if (__ref._M_ptr != this->get())
   {
     delete _M_ptr;
     _M_ptr = __ref._M_ptr;
   }
 return *this;
      }

      template<typename _Tp1>
        operator auto_ptr_ref<_Tp1>() throw()
        { return auto_ptr_ref<_Tp1>(this->release()); }

      template<typename _Tp1>
        operator auto_ptr<_Tp1>() throw()
        { return auto_ptr<_Tp1>(this->release()); }
    } ;



  template<>
    class auto_ptr<void>
    {
    public:
      typedef void element_type;
    } ;
# 338 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/auto_ptr.h" 3
#pragma GCC diagnostic pop


}
# 89 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3
# 117 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 118 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/memory" 2 3
# 6 "./lookup/missing-std-include-8.C" 2


# 7 "./lookup/missing-std-include-8.C"
template<class T>
void test_make_shared ()
{
  std::make_shared<T>();



}

template<class T>
void test_make_unique ()
{
  std::make_unique<T>();



}

void test_array ()
{
  std::array a;

}

void test_tuple ()
{
  std::tuple<int,float> p;


}


std::shared_timed_mutex m;



std::string_view sv;



using namespace std;
void test_via_using_directive ()
{
  shared_ptr<int> p;


}
