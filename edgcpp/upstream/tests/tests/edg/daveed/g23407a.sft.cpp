//remark:MSVC substitution bug
//options:--c++11;fp:--microsoft_bugs --c++11;fn

template<typename T> int f(int (*)[sizeof(&T::init)+1]);
template<typename T> int f(...) = delete;
struct X { void init(); };
int r = f<X>(0);
