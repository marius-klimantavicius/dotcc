/* Distinct EDG array-type nodes can denote compatible C array pointer types. */
typedef char Block[100];
typedef char AnotherBlock[100];
struct Holder { char bytes[100]; };
#ifdef __cplusplus
extern "C" {
#endif
static int consume(char (*p)[100]) { return (*p)[0] + (*p)[99]; }
static int constant_consume(const char (*p)[100]) { return (*p)[0] + (*p)[99]; }
static char (*identity(char (*p)[100]))[100] { return p; }
static char (*choose(int flag,char (*a)[100],char (*b)[100]))[100] { return flag?a:b; }
int array_pointer_cases(void)
{
    char local[100]={0};Block named={0};AnotherBlock other={0};struct Holder holder={{0}};
    char (*p)[100]=&local;int (*indirect)(char (*)[100])=consume;
    local[0]=3;local[99]=7;named[0]=11;named[99]=13;other[0]=17;other[99]=19;holder.bytes[0]=23;holder.bytes[99]=29;
    if(consume(&local)!=10 || consume(&named)!=24 || consume(&other)!=36 || consume(&holder.bytes)!=52)return 1;
    if(indirect(&local)!=10 || indirect(&named)!=24 || indirect(&other)!=36 || indirect(&holder.bytes)!=52)return 2;
    if(identity(&local)!=p || identity(&named)!=&named)return 3;
    p=identity(&holder.bytes);if(consume(p)!=52)return 4;
    p=choose(0,&local,&named);if((*p)[0]!=11 || constant_consume((const char (*)[100])p)!=24)return 5;
    char matrix[2][100]={{0},{0}};matrix[1][0]=31;matrix[1][99]=37;
    p=matrix;if(consume(p+1)!=68 || identity(&matrix[1])!=p+1)return 6;
    return 0;
}
#ifdef __cplusplus
}
#endif
