//type:fp
//options_all:--c++20 -tused
//remark:Spurious constant-evaluation error when capturing an empty class variable
// 3/9/26   [EDGcpfe/28738]
//
// Spurious constant-evaluation error when capturing an empty class variable
//
// Here, line (1) copies an empty lambda as a capture.  Previously, the front end
// issued an error claiming that this involves uninitialized data.  That is now
// fixed.
auto lm = [](auto &&p) { return [p]{}; };
constexpr auto lm_wrap = lm([](auto) {});
