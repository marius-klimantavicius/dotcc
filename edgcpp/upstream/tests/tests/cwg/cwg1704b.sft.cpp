//type:fp
//options_all:--c++17 -tused -A
   template<typename T> auto av = T();
   template int av<int>;        // OK, variable with type int can be redeclared with type auto
