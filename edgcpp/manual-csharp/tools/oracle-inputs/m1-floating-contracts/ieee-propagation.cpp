constexpr double d(double x,double y){return x/y;}constexpr double inf=d(1.0,0.0);constexpr double n=d(0.0,0.0);static_assert(inf>1e300 && inf==inf && n!=n && !(n<0.0) && !(n>=0.0) && bool(n));
