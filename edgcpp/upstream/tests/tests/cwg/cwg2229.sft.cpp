//type:fn
//options_all:--c++17 -tused -A
struct S {
 // three-bit unsigned field,
 // allowed values are 0...7
unsigned int b : 3;
volatile int : 0;
};

//cwg: 2229
//title: Volatile unnamed bit-fields
//meeting: Jacksonville 2/18
//edg_status: EDGcpfe/21959
//fixed_in: 6.4
