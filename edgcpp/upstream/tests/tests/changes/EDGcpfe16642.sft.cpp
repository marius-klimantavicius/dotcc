//type:fp
//remark:[4.11] Discarding function template candidates early
// 11/16/15 [EDGcpfe/16642]
//
// Discarding function template candidates early
//
// When performing overload resolution, the front end now discards function
// templates earlier if it can do so based on just comparing the number of call
// arguments with the parameter list.
//
// Previously, the partial substitution of candidate (1) for the call g<int>()
// triggered an error in the instantiation of S<int> (the expression "*T()" is
// not valid when T is int).  Now, that candidate is discarded early (except in
// GNU C++ mode) because the parameter list cannot match the number of arguments
// in the call.
template<typename T> struct S {
  typedef decltype(*T()) type;
};
template<typename T> typename S<T>::type g(int x); // (1)
template <typename> void g();
int main(){
  g<int>();
}
