//type:fp
//options_all:--microsoft
//remark:[4.13] Microsoft compatibility: comma suppression in macro argument
// 1/18/17  [EDGcpfe/17519]
//
// Microsoft compatibility: comma suppression in macro argument
//
// The Microsoft preprocessor suppresses a comma appearing in a macro argument
// preceding an empty variadic macro expansion.  The front end now emulates
// that behavior in Microsoft mode.
#define M(...) __VA_ARGS__
#define X(...) __VA_ARGS__, b;
#define Y(x,...) X(a, M(__VA_ARGS__))
int Y(xyz)  // Previously expanded to "int a,, b;", now "int a, b;"
