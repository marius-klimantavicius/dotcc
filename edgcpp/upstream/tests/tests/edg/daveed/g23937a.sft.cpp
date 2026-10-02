//remark:Microsoft layout and explicit alignment
//options:--microsoft;fp

#pragma pack(push, 8)
 
template <typename T>
struct tuple_val { T ele; };
 
template <typename T>
struct mytuple_base {  tuple_val<T> base_val;  };
 
template <typename T1, typename T2> struct mytuple: public mytuple_base<T2> {   tuple_val<T1> val; };
 
#pragma pack(pop)
 
struct alignas(16) S1 { char c; };
typedef mytuple<int, S1> targ_t;
 
int main() {
static_assert(alignof(targ_t) == 16, "");
}
