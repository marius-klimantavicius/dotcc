//type:fp
//options_all:--c++20 -tused -A
   template<typename T> T end(T);
   template<typename T>
   bool Foo(T it) {
     return it->end < it->end;
   }
