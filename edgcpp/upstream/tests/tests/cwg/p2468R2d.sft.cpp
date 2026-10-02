//type:fn
//options::-A
//options_all:--c++20 -tused
using ubool = unsigned char;
   
   struct S {
     operator bool() const;
   };
   ubool operator==(S, S);
   
   ubool b = S{} != S{};
