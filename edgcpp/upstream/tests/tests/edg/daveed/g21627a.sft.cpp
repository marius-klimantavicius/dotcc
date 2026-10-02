//remark:GNU using::operator= behavior
//options:--c++14;fn:--c++14 --gnu_version 50400;fp

extern "C" int printf(const char*, ...);
 
template<typename _Tp, _Tp __v>
struct integral_constant {
  static constexpr _Tp value = __v;
  typedef _Tp value_type;
  typedef integral_constant<_Tp, __v> type;
  constexpr operator value_type() const { return value; }
  constexpr value_type operator()() const { return value; }
};
 
typedef integral_constant<bool, true> true_type;
typedef integral_constant<bool, false> false_type;
 
template<typename, typename> struct is_same : public false_type { };
template<typename _Tp> struct is_same<_Tp, _Tp> : public true_type { };
 
template <bool Cond, typename Result=void>
struct enable_if { };
 
template <typename Result>
struct enable_if<true, Result> { using type = Result; };
 
template<bool _Cond, typename _Tp = void>
using enable_if_t = typename enable_if<_Cond, _Tp>::type;
 
struct A {
  template<typename T, typename = enable_if_t<is_same<T, int>::value> >
  A& operator=(T)
  {
    printf("A::foo\n ");
    return *this;
  }
};
 
struct B : public A {
  using A::operator=;
  template<typename T, typename = enable_if_t<!is_same<T, int>::value> >
  B& operator=( T )
  {
    printf("B::foo\n ");
    return *this;
  }
};
 
int main()
{
  B b;
  b = (int) 0;
  b = (float) 0;
}
