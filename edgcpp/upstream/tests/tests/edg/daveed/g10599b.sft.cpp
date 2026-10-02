//remark:auto type checking
//options:--c++0x;fn:--c++0x -A;fn

auto _t1_ = []{};
typedef decltype(_t1_) _A1_;
extern _A1_ x ; 
auto x(37);

