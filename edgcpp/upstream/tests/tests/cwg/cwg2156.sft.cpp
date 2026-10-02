//type:fn
//options_all:--c++17 -tused -A
enum color {red, blue};
 namespace N {
 using ::color;
 }
enum N::color {red, blue};

//cwg: 2156
//title: Definition of enumeration declared by using-declaration
//meeting: Jacksonville 2/16
//edg_status: Passes
