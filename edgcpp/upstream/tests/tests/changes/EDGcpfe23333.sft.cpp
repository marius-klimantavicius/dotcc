//type:fp
//options_all:--gcc
//remark:[6.7] GNU compatibility: Extraneous parentheses in attribute string operands
// 5/31/24  [EDGcpfe/23333,EDGcpfe/24685,EDGcpfe/27190]
//
// GNU compatibility: Extraneous parentheses in attribute string operands
//
// The front end now accepts extraneous parentheses in string operands for
// GNU-style attributes (providing they are matched).
void f(void) __attribute__((visibility("hidden")));
void g(void) __attribute__((visibility(("hidden"))));
void h(void) __attribute__((visibility((((((("hidden")))))))));
