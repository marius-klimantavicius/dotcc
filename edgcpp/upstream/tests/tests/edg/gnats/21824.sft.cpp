//type:fn
//options_all:--microsoft_v 1914 --ms_c++17
template <typename T>
struct S {
                void f(int = 0);
};
 
template <typename T>
void S<T>::f(int = 0) {}
