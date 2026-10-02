//type:fn
//options_all:--microsoft_v 1914 --ms_c++17
template <typename T>
struct B {
              using type = T;
};
 
template <typename T>
struct D : B<T *> {
              using type = B<T *>::type;
};
