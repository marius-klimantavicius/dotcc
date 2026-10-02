//type:fn
//options_all:--c++17 -tused -A
struct S { (S)(()); (~S)(()); };

//cwg: 1848
//title: Parenthesized constructor and destructor declarators
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
