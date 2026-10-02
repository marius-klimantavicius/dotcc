//type:fp
//source_files:cwg2433b.C
//options_all:--c++17 -tused -A
template<class T>
constexpr T pi = T(3.1415926535897932385L);  // variable template

//cwg: 2433
//title: Variable templates in the ODR
//meeting: Belfast 11/19
//edg_status: Passes
