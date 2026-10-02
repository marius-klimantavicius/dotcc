//type:fp
//options_all:--c++20 -tused -A
template <class T> struct Base {
	 Base* p;
};

template <class T> struct Derived: public Base<T> {
 typename Derived::Base* p; // meaning Derived::Base<T> 
}; 
template<class T, template<class> class U = T::Base> struct Third { };
Third<Derived<int> > t; // OK: default argument uses injected-class-name as a template
