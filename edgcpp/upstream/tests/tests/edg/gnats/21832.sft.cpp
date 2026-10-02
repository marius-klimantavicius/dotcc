//type:fn
//options_all:--microsoft_v 1920 --ms_c++17
void g();
 
template <typename T>
struct S {
              constexpr void f();
};
 
template <>
constexpr void S<int>::f() {
              g();
}
