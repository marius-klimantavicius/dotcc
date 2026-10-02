//type:fp
//options_all:--g++ --c++17
//remark:[4.13] C++17 compatibility: Using attribute namespaces without repetition
// 12/12/16 [EDGcpfe/17695]
//
// C++17 compatibility: Using attribute namespaces without repetition
//
// A C++17 feature that introduces a "using" prefix to standard attribute lists
// has been implemented (see P0028R4).
//
// This construct has the same effect as:
//
// The "using" prefix is represented by a new ak_attr_using_prefix attribute that
// back ends should be prepared to handle (and typically ignore since the
// attributes that follow will have the proper namespace name).  This is a slight
// IL CHANGE.
[[ using gnu: noreturn, deprecated ]] void f() { while(1); }
