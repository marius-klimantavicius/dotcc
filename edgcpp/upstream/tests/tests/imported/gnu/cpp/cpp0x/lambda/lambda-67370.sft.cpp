//type: fp
//options: --c++11
// { dg-do compile { target c++11 } }

template <typename ...T>
void expand(T const& ...);
    
template <typename ...T>
void f(T ...t) {
  expand([t]{ }...);
}
