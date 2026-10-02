//type:fp
//options_all:--microsoft
//remark:[4.3] Microsoft C++ compatibility: dllexport and const variables
// 10/12/10 [EDGcpfe/11070]
//
// Microsoft C++ compatibility: dllexport and const variables
//
// In C++, const variables defined in namespace scope normally have linkage by
// default (i.e., as if they were defined with the keyword "static").  In
// Microsoft C++ mode with microsoft_version >= 1400, however, the front end now
// gives const variables defined in namespace scope external linkage by default
// if the variable was declared with __declspec(dllexport).  Previously, such a
// declaration (without an explicit "extern" specifier) was an error because
// __declspec(dllexport) cannot be specified on entities with internal linkage.
__declspec(dllexport) int const x = 3;
  // Previously an error because const variables have internal linkage
  // by default.  Now accepted and x is given external linkage (when
  // microsoft_version >= 1400).
