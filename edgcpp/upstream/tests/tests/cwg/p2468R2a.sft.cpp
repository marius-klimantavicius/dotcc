//options::-A
//options_all:--c++20 -tused
template <typename T>
   struct Base {
     bool operator==(const T&) const;
     bool operator!=(const T&) const;
   };
   
   struct Derived : Base<Derived> { };
   
   bool b = Derived{} == Derived{};
