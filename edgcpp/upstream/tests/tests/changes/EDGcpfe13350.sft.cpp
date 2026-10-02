//type:fp
//remark:[4.6] C++-generating back end: abort when file ends with preprocessing directive
// 11/12/12 [EDGcpfe/13350]
//
// C++-generating back end: abort when file ends with preprocessing directive
//
// In some cases the C++-generating back end could abort dereferencing an
// invalid pointer when the source file ends with a preprocessing directive.
// This is now fixed.
#pragma pack (push, 8)
template<typename T> struct S { };
template struct __declspec(dllexport) S<int>;
#pragma pack (pop)
