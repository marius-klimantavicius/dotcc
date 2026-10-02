//type:fn
//options:-DT1:-DT2:-DT3
//options_all:--c++20 --modules --set_flag skip_module_imports

#ifdef T1
#define import import
#endif

#ifdef T2
#define export export
#endif

#ifdef T3
#define module module
#endif

export module A;
export import foo;
