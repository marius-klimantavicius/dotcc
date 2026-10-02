//remark:Default template arg checking
//options:--c++03 --gnu=70300;fp:--c++03;fp:--c++03 --gnu=70300 -DNEG;fn:--c++03 -A;fn

template <typename T, const T& = T()>
struct ct {};
 
#ifdef NEG
ct<int> v;
#endif
