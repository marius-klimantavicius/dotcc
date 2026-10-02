struct Pair { int a; int b; };
int global=10;
alignas(64) thread_local int count=10;
thread_local int array[3]={1,2,3};
thread_local char local_text[]="abc";
const char* literal="shared\0literal";
int add(int x) { global+=x; return global; }
typedef int (*Function)(int);
Function get_callback() { return add; }
int call(Function f,int x){return f(x);}
int tls_next() {
    if ((reinterpret_cast<unsigned long>(&count)&63)!=0) return -1;
    if(array[2]!=3 || local_text[0]!='a') return -2;
    return ++count;
}
int* tls_address(){return &count;}
const char* text_address(){return literal;}
Pair pair_roundtrip(Pair p){p.b+=p.a; return p;}
int frame_call(Function f) {
    int a[9000], b[9000];
    alignas(128) int scalar=7;
    if ((reinterpret_cast<unsigned long>(&scalar)&127)!=0) return -100;
    a[0]=11; a[8999]=17; b[0]=23; b[8999]=29;
    int result=f(1);
    return result+a[0]+a[8999]+b[0]+b[8999]+scalar;
}
int pair_copy() {Pair a={3,5}; Pair b=a; b=a; return b.a+b.b;}
