//remark:Deduction and empty pack expansions
//options:--c++14 -tused;fp

template<typename R, typename P> R f(P);
template<typename F, typename S, typename... Ts> int f(Ts... x, S y, S z);
auto r = f<int>(42);


