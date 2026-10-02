//options_all:--c++23 -A
  template<class ...T>
  struct Align{
   alignas(T...) unsigned char buffer[128];
  };
  Align<int, short> a;

//cwg: 2717
//title: Pack expansion for alignment-specifier
//meeting: Varna 6/23
//edg_status: Passes
