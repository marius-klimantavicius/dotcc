//remark:Default default constructor
//options:--c++11;fp:--c++11 --g++;fp:--c++11 --microsoft;fp:--c++11 --clang;fn

struct X { X(int); };
struct S {
    S() = default;
   union {
     int i = {};
     X x;
   };
};

S s;

