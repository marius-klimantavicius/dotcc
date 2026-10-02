//remark:Explicit default constructors
//options:--c++14;fn:--microsoft_v=1924;fn

struct S {
   explicit S(float f = 0.0F) {}
};
S s = {};

