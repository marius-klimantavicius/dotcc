//type:fp
//options_all:--c++11
//remark:[4.8] C++11: user-defined literals
// 8/13/13  [EDGcpfe/9252]
//
// C++11: user-defined literals
//
// In C++11 mode, or when --user_defined_literals is specified on the command
// line, the front end now accepts user-defined literals as described in
// C++ Standard Committee document N2765.
long double operator "" _hr(long double h) { return h * 3600; }
long double seconds_in_2_hours = 2.0_hr;
