//type:fp
//options_all:--c++17 -A -tused -A
//source_files:cwg2198b.C
enum E1 { E };
int f() { return E; }

//cwg: 2198
//title: Linkage of enumerators
//meeting: Kona 2/17
//edg_status: Passes
