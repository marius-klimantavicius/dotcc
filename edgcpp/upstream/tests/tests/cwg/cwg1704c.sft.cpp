//type:fn
//options_all:--c++17 -tused -A

   template<typename T> auto f() {}
   template void f<int>();      // error: function with deduced return type redeclared with non-deduced return type (9.1.8.5 [dcl.spec.auto])
