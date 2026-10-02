//remark:Complex type interpretation
//options:--gnu=60100;fp

constexpr __complex__ double cx = { 1.0, 2.0 };
constexpr auto scx = cx*cx;
auto z = scx;

