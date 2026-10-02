//type:fp
//options_all:--microsoft_v 1910
template<class T1, class T2> struct X {};
 
template<typename ... P1, typename ... P2>
int f(X<P1..., P2...>);
 
void g() {
  f<int, float>( {} );
}
