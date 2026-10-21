constexpr double d(double x,double y){return x/y;}constexpr double s(double x){return x-x;}constexpr double n=s(d(1.0,0.0));static_assert(n!=n);
