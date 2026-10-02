//type:fp
//options_all:--microsoft
//remark:[4.12] Microsoft compatibility: commas in unexpanded macro argument text
// 5/19/16  [EDGcpfe/16325]
//
// Microsoft compatibility: commas in unexpanded macro argument text
//
// The Microsoft preprocessor generally treats commas appearing in macro
// arguments as not being macro argument delimiters when the argument appears
// in a macro argument within the expanded text; see the entry of 4/15/06.
// The front end previously only emulated this behavior when the macro
// argument was used in its expanded form in the macro definition.  However,
// the Microsoft preprocessor gives commas this special treatment even if the
// expansion of the macro argument is suppressed by using the parameter as an
// operand of the ## operator.  The front end has now been changed to do the
// same.
#define CAT(a, b) CAT_I(a, )
#define CAT_I(a, b) CAT_II(a##)  // "a" is used in unexpanded form
#define CAT_II(a) a
#define REM(a, b) a, b
struct S { S(int, int); };
S s(CAT(REM(0,0),));  // Previously expanded to "S s(0)" and warned of too
                      // many arguments in the invocation of CAT_II
