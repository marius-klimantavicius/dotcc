//type:fn
//options_all:--c++17 -tused -A
//
#include <initializer_list>
 auto x2{1, 2}; // Was std::initializer_list<int>, now ill-formed

//cwg: 2038
//title: Document C++14 incompatibility of new braced deduction rule
//meeting: Jacksonville 2/16
//edg_status: Passes
