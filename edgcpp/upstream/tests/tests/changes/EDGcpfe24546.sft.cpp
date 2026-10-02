//type:fp
//options_all:--c++20 --g++
//remark:[6.4] IA-64 ABI: [[no_unique_address]] layout
// 2/1/22   [EDGcpfe/24546,EDGcpfe/24819]
//
// IA-64 ABI: [[no_unique_address]] layout
//
// In IA-64 ABI configurations, fields with the [[no_unique_address]] attribute
// and empty class types were not assigned the correct layout in cases with
// three or more consecutive fields of the same type.
// --g++):
struct C {};
struct A {
  [[no_unique_address]] C a1;
  [[no_unique_address]] C a2;
  [[no_unique_address]] C a3;
};
static_assert(__builtin_offsetof(A,a1) == 0);   // Okay.
static_assert(__builtin_offsetof(A,a2) == 1);   // Okay.
static_assert(__builtin_offsetof(A,a3) == 2);   // Had previously failed.
