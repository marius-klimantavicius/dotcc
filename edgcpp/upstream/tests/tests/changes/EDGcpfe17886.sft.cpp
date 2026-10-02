//type:fp
//options_all:--gcc
//remark:[5.1] GNU statement expressions in unevaluated contexts
// 3/1/19   [EDGcpfe/17886,EDGcpfe/19556,EDGcpfe/19680,EDGcpfe/20896]
//
// GNU statement expressions in unevaluated contexts
//
// In GNU modes, the front end now accepts GNU statement expressions in non-
// local scopes when they appear in unevaluated contexts.
unsigned long s = sizeof( ({ 42; }) );  // Now okay in GNU modes.
