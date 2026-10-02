//type:fp
//options_all:--microsoft
//remark:[4.4] Microsoft compatibility: Predeclare std::nullptr_t
// 9/22/11  [EDGcpfe/12159]
//
// Microsoft compatibility: Predeclare std::nullptr_t
//
// Microsoft compilers beginning with MSVC 10 (version 1600) predeclare the
// symbol nullptr_t in namespace std.  The front end now does the same in
// Microsoft mode.
void f(std::nullptr_t);   // Previously diagnosed as an error
