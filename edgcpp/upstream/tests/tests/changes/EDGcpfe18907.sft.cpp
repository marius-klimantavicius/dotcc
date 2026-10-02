//type:fp
//options_all:--microsoft_version 1913
//remark:[6.3] Microsoft compatibility: __declspec(thread) and dynamic initialization
// 9/17/21  [EDGcpfe/18907,EDGcpfe/24687]
//
// Microsoft compatibility: __declspec(thread) and dynamic initialization
//
// In Microsoft modes, the front end previously never allowed a __declspec(thread)
// to have non-constant initialization.
//
// Now that constraint has been lifted when microsoft_version >= 1900.
struct S { ~S(); };
__declspec(thread) S s;  // Previously always an error.  Now okay in
                         // Microsoft mode with microsoft_version >= 1900.
