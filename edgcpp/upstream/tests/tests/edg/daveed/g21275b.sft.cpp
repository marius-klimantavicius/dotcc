//remark:Deduction of auto template parameters
//options:--c++20;fp

template<typename T, auto = T()+42>
int f(T) { return 42; };
int r = f('x');
