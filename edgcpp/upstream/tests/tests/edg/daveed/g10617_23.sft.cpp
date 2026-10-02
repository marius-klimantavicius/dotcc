/* Created on 09/27/12. */
/* GNATS entry EDGcpfe/13180. */
//options_all:--c++11
//options:-A:--g++

//original name from jhs:noexcept006.C

template<typename _Tp> _Tp declval() noexcept;

// is_nothrow_move_constructible-ish
template<typename _Tp , typename = decltype(_Tp(declval<_Tp&&>()))>
struct trait
{
   static const bool value=true;
};

template<class _T2>
struct pair
{
   _T2 second;
   void swap(pair& __p) noexcept(trait<_T2>::value);
};

template < class R_ >
struct Main
{
   Main() {}
   Main(const typename R_::Sub1T&);
   Main(const typename R_::Sub2T&);
};

template < class R_ >
class Sub1
{
   typedef pair<typename R_::MainT> Rep;
   Rep base;
};

template < class R_ >
struct Sub2
{
   typedef pair<typename R_::MainT> Rep;
   Rep base;
};

struct Kernel
{
   typedef Main<Kernel> MainT;
   typedef Sub1<Kernel> Sub1T;
   typedef Sub2<Kernel> Sub2T;
};

Main<Kernel> f()
{
   return Main<Kernel> ();
}
