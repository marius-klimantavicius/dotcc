//type:fp
//options_all:--c++17 -tused -A
inline namespace X {
  template<typename T> struct Y {};
}
template<> struct Y<int> {}; // ok by 7.3.1/8
template<typename T> struct Y<T*> {}; // ok?
