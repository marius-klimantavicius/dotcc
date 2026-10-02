//remark: Microsoft __declspec(nothrow)
//options:--microsoft_v=1934 --ms_c++17;fp:--microsoft_v=1934 --ms_c++17 -DNEG;fn

           struct B {
             virtual __declspec(nothrow) void e();
#ifdef NEG
             virtual __declspec(nothrow) void f() noexcept;
#endif
           };
           struct D: B {
             void e();  // Okay.
#ifdef NEG
             void f();  // Error.
#endif
           };
