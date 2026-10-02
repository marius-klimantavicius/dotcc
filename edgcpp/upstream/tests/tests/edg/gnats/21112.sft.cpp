//type:fp
//options_all::--ms_c++latest --microsoft_version=1903
template<auto> struct wow;
template<typename T, typename R, R (*F)(T const&)>
struct wow<F>
{
    R DoIt(T const& t) { return F(t);}
};
