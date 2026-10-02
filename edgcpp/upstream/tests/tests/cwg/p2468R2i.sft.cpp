//type:fn
//options_all:-tused --c++20
template<class Derived>
   struct Base {
     int operator==(const double&) const;
     friend inline int operator==(const double&, const Derived&);
   };
   
   struct X : Base<X> { };
   
   bool b = X{} == 0.;
