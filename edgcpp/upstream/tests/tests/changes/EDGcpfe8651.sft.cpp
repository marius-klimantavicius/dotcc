//type:fp
//options_all:--microsoft
//remark:[4.0] Microsoft compatibility: Typedefs for class and enum types
// 9/8/08   [EDGcpfe/8651]
//
// Microsoft compatibility: Typedefs for class and enum types
//
// In Microsoft C++ mode with Microsoft bugs mode enabled (the default), a
// typedef for a named class or enum to that same name (and declared in the same
// scope) is marked as invisible and does not affect later declarations.
typedef struct S {} S;
double S = 1.0;  // Now accepted in Microsoft C++ bugs mode.
