//remark:Complex type interpretation
//options:--gnu=60100;fp

struct S {
   constexpr S() : v{} {}
   S& operator=(float)
   {
       return *this;
   }
   __complex__ float v;
};

constexpr S s;
S ss = s;
