//remark:constinit and destructors
//options:--c++20;fn

constinit struct S { constexpr S() {} int i; } x;
