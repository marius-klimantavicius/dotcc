//type:fn
//options_all:--c++20 -tused -A
   template<typename T> struct S { typedef char I; };
   enum E: S<E>::I { e };   // Implementations say E is undeclared in S<E>

//cwg: 2516
//title: Locus of enum-specifier or opaque-enum-declaration
//meeting: Issaquah 2/23
//edg_status: Passes
