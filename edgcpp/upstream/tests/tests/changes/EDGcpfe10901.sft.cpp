//type:fp
//options_all:--microsoft
//remark:[4.2] Microsoft compatibility: __declspec(deprecated(...)) with wide strings
// 8/11/10  [EDGcpfe/10901]
//
// Microsoft compatibility: __declspec(deprecated(...)) with wide strings
//
// In Microsoft mode with microsoft_version >= 1400, the front end accepts
// __declspec(deprecated(...)) with a string literal argument (see the entry of
__declspec(deprecated(L"Old")) int x;
int y = x;  // Previously triggered a strange diagnostic.
