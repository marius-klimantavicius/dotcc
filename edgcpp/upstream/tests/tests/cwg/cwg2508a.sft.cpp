//type:fn
//options_all:--c++20 -tused -A
  template<class X> class X; // error: hidden by template-parameter

//cwg: 2508
//title: Restrictions on uses of template parameter names
//meeting: Kona 11/22
//edg_status: Passes
