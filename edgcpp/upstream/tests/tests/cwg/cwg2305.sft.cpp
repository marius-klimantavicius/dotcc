//type:fn
//options_all:--c++17 -tused -A

template<typename T> T var = {};
   template inline int var<int>;    

//cwg: 2305
//title: Explicit instantiation of constexpr or inline variable template
//meeting: Albuquerque 11/17
//edg_status: Passes
