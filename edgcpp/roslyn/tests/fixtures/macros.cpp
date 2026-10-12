#define INNER(x) ((x)+1)
#define OUTER(x) INNER(x)
#define INC(x) OUTER(x)
int first(int x){return INC(x);}
#undef INC
#define INC(x) ((x)+2)
#line 200 "logical.cpp"
int second(int x){return INC(x);}
