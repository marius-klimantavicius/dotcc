//type:fn
//options_all:--c++20 -tused
struct Base {
       bool operator==(const Base&) const;
       bool operator!=(const Base&) const;
   };
   
   struct Derived : Base {
       Derived(const Base&);
       bool operator==(const Derived& rhs) const {
           return static_cast<const Base&>(*this) == rhs;
       }
   };
