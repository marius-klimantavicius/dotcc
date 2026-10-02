int f(), x;            // OK, function declaration for f and object declaration for x
extern void g();       // OK, function declaration for g

//cwg: 2475
//title: Object declarations of type cv void
//meeting: Issaquah 2/23
//edg_status: Passes
