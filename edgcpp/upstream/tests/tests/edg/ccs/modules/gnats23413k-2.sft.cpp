//type:fp
//options_all:--c++20 --modules --set_flag skip_module_imports

#define import 1
#undef import
export module A;
export import foo;
