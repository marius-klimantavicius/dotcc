//type:cp
//options:--gnu_version 70400;fn:--gnu_version 80100
//fixing_pr:22112
struct S { union { int a; }; };

struct S s = { .a = 0 };
