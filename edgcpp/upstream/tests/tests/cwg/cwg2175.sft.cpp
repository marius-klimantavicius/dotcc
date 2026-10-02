//type:fn
//options_all:--c++17 -tused -A
//

operator int [[noreturn]] (); // error: noreturn attribute applied to a type

//cwg: 2175
//title: Ambiguity with attribute in conversion operator declaration
//meeting: Jacksonville 2/16
//edg_status: Passes
