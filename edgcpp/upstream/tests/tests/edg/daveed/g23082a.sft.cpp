//remark:Vector shift-assign
//options:--gnu=70300 --c++14;fp

template <int N, typename T>
struct VecHelper { typedef T __attribute__((vector_size(N*sizeof(T)))) V; };

template <int N, typename T> using Vec = typename VecHelper<N,T>::V;

using U64 = Vec<8,unsigned long>;

void foo (U64 *v)
{
  *v = *v >> 16; // ok
  *v >>= 16;     // fail
}
