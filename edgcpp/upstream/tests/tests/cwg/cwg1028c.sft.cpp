//type:fp
//options_all:--c++20 -tused -A 
struct A {operator int();};
template<class B,class T>
struct D : B {
  T get() {return operator T();}     // conversion-function-id is dependent
};
int f(D<A,int> d) {return d.get();}  // OK: lookup finds A::operator int
