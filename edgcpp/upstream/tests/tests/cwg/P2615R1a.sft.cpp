//type:fp
//options_all:--c++20 -tused  -A
export module M;
export int f();                 // OK
export namespace N { }          // OK
export using namespace N;       // OK
