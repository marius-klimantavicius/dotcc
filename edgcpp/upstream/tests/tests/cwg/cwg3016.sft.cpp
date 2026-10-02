//type:fn
//options:--c++23 -A

#if __has_include(<a> b)
#endif

#if __has_include(a)
#endif

#define NAME a
#if __has_include(NAME)
#endif

//cwg: 3016
//title: Satisfying the syntactic requirements of #include and #embed
//meeting: Sofia 6/25
//edg_status: Passes
