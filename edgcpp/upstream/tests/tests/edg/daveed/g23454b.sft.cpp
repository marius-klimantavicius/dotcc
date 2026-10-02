//remark:Default template arg checking

//options_all:--c++17
int n;
template<auto A, decltype(A) B = &n> struct SubstFailure {}; //should work, but is rejected

SubstFailure<&n> s;
