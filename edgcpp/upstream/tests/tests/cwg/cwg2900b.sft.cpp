//type:fn
//options:--c++23
//options_all:-A

template<int &> struct E;
template<auto x> void f(E<x> *);
int v;
void g(E<v> *bp) {
  f(bp); // error: type int of x does not match the int & type of the template parameter in the E<v> specialization of E
}
