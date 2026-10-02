//remark:C++23 multi-subscript
//options:--c++23;fp

 template<typename T>
 void f(T t)
 {
   t[1];
   t[1, 2];
 }

