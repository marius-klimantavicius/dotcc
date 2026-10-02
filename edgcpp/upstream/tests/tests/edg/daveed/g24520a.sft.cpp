//remark:Microsoft __restrict parameters
//options:--microsoft_v=1928;fp

template <typename T1, typename T2>
struct is_same { static const bool value = false; };
 
template<typename T>
struct is_same<T,T> { static const bool value = true; };
 
 
using type = float * __restrict ;
 
void foo(type a) {
static_assert(!is_same<decltype(foo) *, void (*)(float *) >::value, "");
}
