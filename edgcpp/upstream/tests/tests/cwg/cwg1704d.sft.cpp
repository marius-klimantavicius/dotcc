//type:fp
//options_all:--c++17 -tused -A
   template<typename T> T var = {};
   template float var<float>;   // OK, instantiated variable has type float
   template int var<int[16]>[]; // OK, absence of major array bound is permitted
