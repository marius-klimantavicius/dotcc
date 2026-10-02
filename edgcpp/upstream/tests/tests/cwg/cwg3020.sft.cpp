//type:fp
//options:--c++26 -A

#if !__has_cpp_attribute(indeterminate)
#error no attribute indeterminate
#endif

//cwg: 3020
//title: Missing specification for __has_cpp_attribute(indeterminate)
//meeting: Sofia 6/25
//edg_status: EDGcpfe/28249
