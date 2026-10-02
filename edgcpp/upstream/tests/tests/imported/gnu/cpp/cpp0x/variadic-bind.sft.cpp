//type: rp
//options: --c++11
# 0 "./cpp0x/variadic-bind.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp0x/variadic-bind.C"




# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 1 3
# 45 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 3
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
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 2 3
# 1 "/usr/include/assert.h" 1 3 4
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
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 2 3
# 6 "./cpp0x/variadic-bind.C" 2



# 8 "./cpp0x/variadic-bind.C"
template<typename T>
struct reference_wrapper
{
  reference_wrapper(T& x) : ptr(&x) { }

  operator T&() const { return *ptr; }

  T& get() const { return *ptr; }

  T* ptr;
};

template<typename T> reference_wrapper<T> ref(T& x) { return x; }
template<typename T> reference_wrapper<const T> cref(const T& x) { return x; }


template<typename T>
struct add_reference
{
  typedef T& type;
};

template<typename T>
struct add_reference<T&>
{
  typedef T& type;
};

template<typename T, typename U>
struct is_same
{
  static const bool value = false;
};

template<typename T>
struct is_same<T, T>
{
  static const bool value = true;
};


template<typename T>
struct add_const_reference
{
  typedef const T& type;
};

template<typename T>
struct add_const_reference<T&>
{
  typedef T& type;
};


template<typename... Values>
class tuple;

template<> class tuple<> { };

template<typename Head, typename... Tail>
class tuple<Head, Tail...>
  : private tuple<Tail...>
{
  typedef tuple<Tail...> inherited;

 public:
  tuple() { }



  tuple(typename add_const_reference<Head>::type v,
        typename add_const_reference<Tail>::type... vtail)
    : m_head(v), inherited(vtail...) { }

  template<typename... VValues>
  tuple(const tuple<VValues...>& other)
    : m_head(other.head()), inherited(other.tail()) { }

  template<typename... VValues>
  tuple& operator=(const tuple<VValues...>& other)
  {
    m_head = other.head();
    tail() = other.tail();
    return *this;
  }

  typename add_reference<Head>::type head() { return m_head; }
  typename add_reference<const Head>::type head() const { return m_head; }
  inherited& tail() { return *this; }
  const inherited& tail() const { return *this; }

 protected:
  Head m_head;
};

template<typename T>
struct make_tuple_result
{
  typedef T type;
};

template<typename T>
struct make_tuple_result<reference_wrapper<T> >
{
  typedef T& type;
};


struct ignore_t {
  template<typename T> ignore_t& operator=(const T&) { return *this; }
} ignore;

template<typename... Values>
tuple<typename make_tuple_result<Values>::type...>
make_tuple(const Values&... values)
{
  return tuple<typename make_tuple_result<Values>::type...>(values...);
}

template<typename... Values>
tuple<Values&...> tie(Values&... values)
{
  return tuple<Values&...>(values...);
}


template<typename Tuple>
struct tuple_size;

template<>
struct tuple_size<tuple<> >
{
  static const long unsigned int value = 0;
};

template<typename Head, typename... Tail>
struct tuple_size<tuple<Head, Tail...> >
{
  static const long unsigned int value = 1 + tuple_size<tuple<Tail...> >::value;
};

template<int I, typename Tuple>
struct tuple_element;

template<int I, typename Head, typename... Tail>
struct tuple_element<I, tuple<Head, Tail...> >
{
  typedef typename tuple_element<I-1, tuple<Tail...> >::type type;
};

template<typename Head, typename... Tail>
struct tuple_element<0, tuple<Head, Tail...> >
{
  typedef Head type;
};


template<int I, typename Tuple>
class get_impl;

template<int I, typename Head, typename... Values>
class get_impl<I, tuple<Head, Values...> >
{
  typedef typename tuple_element<I-1, tuple<Values...> >::type Element;
  typedef typename add_reference<Element>::type RJ;
  typedef typename add_const_reference<Element>::type PJ;
  typedef get_impl<I-1, tuple<Values...> > Next;

 public:
  static RJ get(tuple<Head, Values...>& t)
  { return Next::get(t.tail()); }

  static PJ get(const tuple<Head, Values...>& t)
  { return Next::get(t.tail()); }
};

template<typename Head, typename... Values>
class get_impl<0, tuple<Head, Values...> >
{
  typedef typename add_reference<Head>::type RJ;
  typedef typename add_const_reference<Head>::type PJ;

 public:
  static RJ get(tuple<Head, Values...>& t) { return t.head(); }
  static PJ get(const tuple<Head, Values...>& t) { return t.head(); }
};

template<int I, typename... Values>
typename add_reference<
           typename tuple_element<I, tuple<Values...> >::type
         >::type
get(tuple<Values...>& t)
{
  return get_impl<I, tuple<Values...> >::get(t);
}

template<int I, typename... Values>
typename add_const_reference<
           typename tuple_element<I, tuple<Values...> >::type
         >::type
get(const tuple<Values...>& t)
{
  return get_impl<I, tuple<Values...> >::get(t);
}


inline bool operator==(const tuple<>&, const tuple<>&) { return true; }

template<typename T, typename... TTail, typename U, typename... UTail>
bool operator==(const tuple<T, TTail...>& t, const tuple<U, UTail...>& u)
{
  return t.head() == u.head() && t.tail() == u.tail();
}

template<typename... TValues, typename... UValues>
bool operator!=(const tuple<TValues...>& t, const tuple<UValues...>& u)
{
  return !(t == u);
}

inline bool operator<(const tuple<>&, const tuple<>&) { return false; }

template<typename T, typename... TTail, typename U, typename... UTail>
bool operator<(const tuple<T, TTail...>& t, const tuple<U, UTail...>& u)
{
  return (t.head() < u.head() ||
          (!(t.head() < u.head()) && t.tail() < u.tail()));
}

template<typename... TValues, typename... UValues>
bool operator>(const tuple<TValues...>& t, const tuple<UValues...>& u)
{
  return u < t;
}

template<typename... TValues, typename... UValues>
bool operator<=(const tuple<TValues...>& t, const tuple<UValues...>& u)
{
  return !(u < t);
}

template<typename... TValues, typename... UValues>
bool operator>=(const tuple<TValues...>& t, const tuple<UValues...>& u)
{
  return !(t < u);
}


template<bool Cond, typename Type = void>
struct enable_if {
  typedef Type type;
};

template<typename Type>
struct enable_if<false, Type> { };




template<typename T>
struct is_bind_expression {
  static const bool value = false;
};


template<typename T>
struct is_placeholder {
  static const int value = 0;
};


template<int I> struct placeholder {} ;

template<int N> struct int_c { };


template<int...> struct int_tuple {};


template<int I, typename IntTuple, typename... Types>
struct make_indexes_impl;


template<int I, int... Indexes, typename T, typename... Types>
struct make_indexes_impl<I, int_tuple<Indexes...>, T, Types...>
{
  typedef typename make_indexes_impl<I+1,
                                     int_tuple<Indexes..., I>,
                                     Types...>::type type;
};

template<int I, int... Indexes>
struct make_indexes_impl<I, int_tuple<Indexes...> > {
  typedef int_tuple<Indexes...> type;
};




template<typename... Types>
struct make_indexes : make_indexes_impl<0, int_tuple<>, Types...> { };


template<int I, typename Tuple, typename = void>
struct safe_tuple_element{ };

template<int I, typename... Values>
struct safe_tuple_element<I, tuple<Values...>,
         typename enable_if<(I >= 0 &&
                             I < tuple_size<tuple<Values...> >::value)
                            >::type>
{
  typedef typename tuple_element<I, tuple<Values...> >::type type;
};





template<typename T, typename... Args>
inline T& mu(reference_wrapper<T>& bound_arg, const tuple<Args&...>&)
{
  return bound_arg.get();
}



template<typename F, int... Indexes, typename... Args>
inline typename F::result_type
unwrap_and_forward(F& f, int_tuple<Indexes...>, const tuple<Args&...>& args)
{
  return f(get<Indexes>(args)...);
}


template<typename Bound, typename... Args>
inline typename enable_if<is_bind_expression<Bound>::value,
                          typename Bound::result_type>::type
mu(Bound& bound_arg, const tuple<Args&...>& args)
{
  typedef typename make_indexes<Args...>::type Indexes;
  return unwrap_and_forward(bound_arg, Indexes(), args);
}


template<typename Bound, typename... Args>
inline typename safe_tuple_element<is_placeholder<Bound>::value - 1,
                                   tuple<Args...> >::type
mu(Bound& bound_arg, const tuple<Args&...>& args)
{
  return get<is_placeholder<Bound>::value-1>(args);
}


template<typename T>
struct is_reference_wrapper {
  static const bool value = false;
};

template<typename T>
struct is_reference_wrapper<reference_wrapper<T> > {
  static const bool value = true;
};

template<typename Bound, typename... Args>
inline typename enable_if<(!is_bind_expression<Bound>::value
                           && !is_placeholder<Bound>::value
                           && !is_reference_wrapper<Bound>::value),
                          Bound&>::type
mu(Bound& bound_arg, const tuple<Args&...>&)
{
  return bound_arg;
}


template<typename F, typename... BoundArgs, int... Indexes, typename... Args>
typename F::result_type
apply_functor(F& f, tuple<BoundArgs...>& bound_args, int_tuple<Indexes...>,
              const tuple<Args&...>& args)
{
  return f(mu(get<Indexes>(bound_args), args)...);
}

template<typename F, typename... BoundArgs>
class bound_functor
{
  typedef typename make_indexes<BoundArgs...>::type indexes;

 public:
  typedef typename F::result_type result_type;

  explicit bound_functor(const F& f, const BoundArgs&... bound_args)
    : f(f), bound_args(bound_args...) { }

  template<typename... Args>
  typename F::result_type operator()(Args&... args) {
    return apply_functor(f, bound_args, indexes(), tie(args...));
  }

 private:
  F f;
  tuple<BoundArgs...> bound_args;
};

template<typename F, typename... BoundArgs>
struct is_bind_expression<bound_functor<F, BoundArgs...> > {
  static const bool value = true;
};

template<typename F, typename... BoundArgs>
inline bound_functor<F, BoundArgs...>
bind(const F& f, const BoundArgs&... bound_args)
{
  return bound_functor<F, BoundArgs...>(f, bound_args...);
}



template<int I>
struct is_placeholder<placeholder<I> > {
  static const int value = I;
};

placeholder<1> _1;
placeholder<2> _2;
placeholder<3> _3;
placeholder<4> _4;
placeholder<5> _5;
placeholder<6> _6;
placeholder<7> _7;
placeholder<8> _8;
placeholder<9> _9;


template<typename T>
struct plus {
  typedef T result_type;

  T operator()(T x, T y) { return x + y; }
};

template<typename T>
struct multiplies {
  typedef T result_type;

  T operator()(T x, T y) { return x * y; }
};

template<typename T>
struct negate {
  typedef T result_type;

  T operator()(T x) { return -x; }
};

int main()
{
  int seventeen = 17;
  int forty_two = 42;

  
# 468 "./cpp0x/variadic-bind.C" 3 4
 ((
# 468 "./cpp0x/variadic-bind.C"
 bind(plus<int>(), _1, _2)(seventeen, forty_two) == 59
# 468 "./cpp0x/variadic-bind.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 468 "./cpp0x/variadic-bind.C"
 "bind(plus<int>(), _1, _2)(seventeen, forty_two) == 59"
# 468 "./cpp0x/variadic-bind.C" 3 4
 , "./cpp0x/variadic-bind.C", 468, __PRETTY_FUNCTION__))
# 468 "./cpp0x/variadic-bind.C"
                                                              ;
  
# 469 "./cpp0x/variadic-bind.C" 3 4
 ((
# 469 "./cpp0x/variadic-bind.C"
 bind(plus<int>(), _1, _1)(seventeen, forty_two) == 34
# 469 "./cpp0x/variadic-bind.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 469 "./cpp0x/variadic-bind.C"
 "bind(plus<int>(), _1, _1)(seventeen, forty_two) == 34"
# 469 "./cpp0x/variadic-bind.C" 3 4
 , "./cpp0x/variadic-bind.C", 469, __PRETTY_FUNCTION__))
# 469 "./cpp0x/variadic-bind.C"
                                                              ;
  
# 470 "./cpp0x/variadic-bind.C" 3 4
 ((
# 470 "./cpp0x/variadic-bind.C"
 bind(plus<int>(), _2, _1)(seventeen, forty_two) == 59
# 470 "./cpp0x/variadic-bind.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 470 "./cpp0x/variadic-bind.C"
 "bind(plus<int>(), _2, _1)(seventeen, forty_two) == 59"
# 470 "./cpp0x/variadic-bind.C" 3 4
 , "./cpp0x/variadic-bind.C", 470, __PRETTY_FUNCTION__))
# 470 "./cpp0x/variadic-bind.C"
                                                              ;
  
# 471 "./cpp0x/variadic-bind.C" 3 4
 ((
# 471 "./cpp0x/variadic-bind.C"
 bind(plus<int>(), 5, _1)(seventeen, forty_two) == 22
# 471 "./cpp0x/variadic-bind.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 471 "./cpp0x/variadic-bind.C"
 "bind(plus<int>(), 5, _1)(seventeen, forty_two) == 22"
# 471 "./cpp0x/variadic-bind.C" 3 4
 , "./cpp0x/variadic-bind.C", 471, __PRETTY_FUNCTION__))
# 471 "./cpp0x/variadic-bind.C"
                                                             ;
  
# 472 "./cpp0x/variadic-bind.C" 3 4
 ((
# 472 "./cpp0x/variadic-bind.C"
 bind(plus<int>(), ref(seventeen), _2)(seventeen, forty_two) == 59
# 472 "./cpp0x/variadic-bind.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 472 "./cpp0x/variadic-bind.C"
 "bind(plus<int>(), ref(seventeen), _2)(seventeen, forty_two) == 59"
# 472 "./cpp0x/variadic-bind.C" 3 4
 , "./cpp0x/variadic-bind.C", 472, __PRETTY_FUNCTION__))
# 472 "./cpp0x/variadic-bind.C"
                                                                          ;
  
# 473 "./cpp0x/variadic-bind.C" 3 4
 ((
# 473 "./cpp0x/variadic-bind.C"
 bind(plus<int>(), bind(multiplies<int>(), 3, _1), _2)(seventeen, forty_two) == 93
# 473 "./cpp0x/variadic-bind.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 473 "./cpp0x/variadic-bind.C"
 "bind(plus<int>(), bind(multiplies<int>(), 3, _1), _2)(seventeen, forty_two) == 93"
# 473 "./cpp0x/variadic-bind.C" 3 4
 , "./cpp0x/variadic-bind.C", 473, __PRETTY_FUNCTION__))
               
# 474 "./cpp0x/variadic-bind.C"
              ;
  return 0;
}
