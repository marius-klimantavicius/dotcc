//type:fp
//options_all:--c++14 --c++11 --microsoft
//remark:[6.7] reinterpret_cast from std::nullptr_t
// 8/28/24  [EDGcpfe/26875]
//
// reinterpret_cast from std::nullptr_t
//
// Previously, the front end issued a spurious error on a dependent
// reinterpret_cast from a value of type std::nullptr_t.
// --c++14:
//
// Additionally, in Microsoft mode, a reinterpret_cast from a std::nullptr_t type
// to a pointer or pointer-to-member type is now also accepted.
// --c++11 --microsoft:
void *v = reinterpret_cast<void *>(nullptr);  // Now accepted in Microsoft
                                              // mode.
