typedef __builtin_va_list va_list;
int destroyed;
struct Value {
 int value;
 explicit Value(int x):value(x){}
 Value(const Value& x):value(x.value){}
 ~Value(){++destroyed;}
};
Value make(int count,...){va_list args;__builtin_va_start(args,count);int total=0;for(int i=0;i<count;++i)total+=__builtin_va_arg(args,int);__builtin_va_end(args);return Value(total);}
int use(Value fixed,int count,...){va_list args;__builtin_va_start(args,count);int total=fixed.value;for(int i=0;i<count;++i)total+=__builtin_va_arg(args,int);__builtin_va_end(args);return total;}
int main(){ {Value a=make(3,7,11,13);if(a.value!=31)return 1;auto function=&make;Value b=function(2,17,19);if(b.value!=36)return 2;if(use(a,2,3,5)!=39)return 3;}return destroyed!=3;}
