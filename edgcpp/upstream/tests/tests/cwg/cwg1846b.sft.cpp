//type:fn
//options_all:--c++17 -tused -A
struct S {
S(int a = 0) = default; // ill-formed: default argument
};
