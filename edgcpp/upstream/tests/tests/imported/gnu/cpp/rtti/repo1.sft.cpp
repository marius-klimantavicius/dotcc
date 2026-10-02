//type: fp
//options: 
# 1 "./rtti/repo1.C"
# 1 "<built-in>"
# 1 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 1 "<command-line>" 2
# 1 "./rtti/repo1.C"






# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/typeinfo" 1 3
# 32 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/typeinfo" 3
       
# 33 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/typeinfo" 3

# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/bits/exception.h" 1 3
# 34 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/bits/exception.h" 3
       
# 35 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/bits/exception.h" 3

#pragma GCC visibility push(default)

# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 1 3
# 252 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 3

# 252 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  typedef long unsigned int size_t;
  typedef long int ptrdiff_t;


  typedef decltype(nullptr) nullptr_t;

}
# 274 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
namespace __gnu_cxx
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
# 524 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/os_defines.h" 1 3
# 39 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/os_defines.h" 3
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
# 40 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/os_defines.h" 2 3
# 525 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3


# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/cpu_defines.h" 1 3
# 528 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3
# 39 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/bits/exception.h" 2 3

extern "C++" {

namespace std
{
# 60 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/bits/exception.h" 3
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

#pragma GCC visibility pop
# 35 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/typeinfo" 2 3

# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/bits/hash_bytes.h" 1 3
# 33 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/bits/hash_bytes.h" 3
       
# 34 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/bits/hash_bytes.h" 3



namespace std
{







  size_t
  _Hash_bytes(const void* __ptr, size_t __len, size_t __seed);





  size_t
  _Fnv_hash_bytes(const void* __ptr, size_t __len, size_t __seed);


}
# 37 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/typeinfo" 2 3


#pragma GCC visibility push(default)

extern "C++" {

namespace __cxxabiv1
{
  class __class_type_info;
}
# 80 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/typeinfo" 3
namespace std
{






  class type_info
  {
  public:




    virtual ~type_info();



    const char* name() const noexcept
    { return __name[0] == '*' ? __name + 1 : __name; }
# 115 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/typeinfo" 3
    bool before(const type_info& __arg) const noexcept
    { return (__name[0] == '*' && __arg.__name[0] == '*')
 ? __name < __arg.__name
 : __builtin_strcmp (__name, __arg.__name) < 0; }

    bool operator==(const type_info& __arg) const noexcept
    {
      return ((__name == __arg.__name)
       || (__name[0] != '*' &&
    __builtin_strcmp (__name, __arg.__name) == 0));
    }
# 136 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/typeinfo" 3
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

    type_info& operator=(const type_info&);
    type_info(const type_info&);
  };







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
# 8 "./rtti/repo1.C" 2

# 8 "./rtti/repo1.C"
template<int>
struct function1
{
  function1()
  {
    typeid(int[100]);
  }
};
function1<1> b;

int main () {}
