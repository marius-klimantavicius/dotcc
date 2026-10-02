//type:fn
//options_all:--c++17 -tused -A
//
template<class T1 = int, class T2> class B; // error

//cwg: 2032
//title: Default template-arguments of variable templates
//meeting: Jacksonville 2/16
//edg_status: Passes
