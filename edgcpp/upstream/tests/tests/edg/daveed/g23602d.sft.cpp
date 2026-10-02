//remark:CLang partial spec matching
//options:--c++14 --clang_v=70000;fn

template<int M> struct I {};
template<typename> struct S;
template<short N> void f(I<N>) {}
int main() {
  I<100> i;
  f(i);
}
