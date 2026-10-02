//type:fp
//options_all:--c++20 -tused
template <class T> struct B {
  template <class T2> struct C { };
};

// OKdeprecated: T::template C would be assumed to names a class template:
template <class T, template <class X> class TT = T::template C> struct D { };
D<B<int> > db;

//cwg: 1478
//title: template keyword for dependent template template arguments
//meeting: Virtual 11/20*
//edg_status: Passes
