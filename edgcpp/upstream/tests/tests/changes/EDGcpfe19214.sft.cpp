//type:fp
//options_all:--c++17
//remark:[5.0] Source sequence entries for constexpr static data members
// 1/25/18  [EDGcpfe/19214]
//
// Source sequence entries for constexpr static data members
//
// In C++17 mode, declaring a constexpr static data member with an initializer in
// a class definition and following it by an out-of-class declaration of that same
// member could result in an internal error in some configurations with the macro
// GENERATE_SOURCE_SEQUENCE_LISTS set to TRUE.
//
// (The internal error could occur in reconcile_static_data_member_types or in the
// C++-generating back end.)  That is now fixed.
struct S { constexpr static int m = 3; };
constexpr int S::m;  // Previously could trigger an internal error.
