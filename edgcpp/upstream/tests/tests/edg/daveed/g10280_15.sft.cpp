//remark:GNU attributes
//options:--g++;cp

    namespace std {
       namespace debug {
         template <class T> struct A { };
       }
       using namespace debug __attribute ((__strong__));
       template <> struct A<int> { };   // ok to specialize
     
       template <class T> void f (A<T>);
     }
     
     int main()
     {
       f (std::A<float>());             // lookup finds std::f
       f (std::A<int>());
     }
