//type:fn
//options_all:--c++17 -tused -A

template<typename T> T var = {};
   template int *var<int>;      // error: instantiated variable has type int
