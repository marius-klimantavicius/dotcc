//type:fp
//options_all:--c++20 -tused -A
//source_files:cwg338.C
    typedef struct {} S;
    extern S *q;

//cwg: 338
//title: numerator name with linkage used as class name in other translation unit
//meeting: Virtual 11/20*
//edg_status: Passes
