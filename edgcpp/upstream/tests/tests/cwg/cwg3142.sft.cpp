//type:fp
//options: -A --c++20

#line 100 "cwg3142.C"
#define CAT(a, b) a##b
#define ID CAT(line_, __LINE__)
int ID;

//cwg: 3142
//title: Possible expansions of __LINE__ changing over time
//meeting: Croydon 3/26
//edg_status: Passes
