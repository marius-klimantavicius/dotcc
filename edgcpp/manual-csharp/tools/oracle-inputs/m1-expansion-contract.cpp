// PP-only contract: identifiers intentionally remain outside declarations.
#define F(x) x
#define A F
#define ID(x) x
prescan F(F(7))
inert F(F)(7)
alias A(8)
tail ID(F)(9)
#define EMPTY
#define V(...) __VA_OPT__(1)
expanded_empty V(EMPTY)
nonempty V(2)
#define CAT(a,b) a ## b
#define STR(a) #a
pasted CAT(to,ken)
empty_paste CAT(,right) CAT(left,) CAT(,)
stringized STR(a   + b)
#define PLUS +
adjacent PLUS +
#define LESS <
adjacent_less LESS <
#define UNUSED 1e+
#define SPLICED\
(x) x
spliced SPLICED(10)
#define COMMENT/**/(x) x
comment COMMENT(11)
physical_line __LINE__ \
+ __LINE__
#if 1 || (1 / 0)
short_circuit yes
#endif
#if 0 ? 1 / 0 : 1
conditional yes
#endif
