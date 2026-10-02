typedef void (*Hook)(int);
Hook active_hook;
unsigned long long trace;
int values[4]={10,20,30,40};
void event(int n){trace=trace*10+static_cast<unsigned>(n);active_hook(n);}
void reset(Hook h){trace=0;active_hook=h;}
unsigned long long read_trace(){return trace;}
struct X {
    int value; const X* self;
    X(int n):value(n),self(this){event(n);}
    X(const X& x):value(x.value),self(this){event(4);}
    ~X() noexcept(false){if(self!=this)event(0);event(value+5);}
    operator int() const {event(4);return value;}
    X& operator=(const X& x){event(4);value=x.value;return *this;}
    int* pointer(){event(4);return values;}
    int member(const X& a,const X& b) const {event(9);return value+a.value+b.value;}
};
void inspect(const X& x){if(x.self!=&x)event(0);}
int two(const X& a,const X& b){inspect(a);inspect(b);event(9);return a.value+b.value;}
int three(const X& a,const X& b,const X& c){inspect(a);inspect(b);inspect(c);event(9);return a.value+b.value+c.value;}
int by_value(X a,X b){inspect(a);inspect(b);event(9);return a.value+b.value;}
int operator+(const X& a,const X& b){return two(a,b);}
X make(const X& a,const X& b){inspect(a);inspect(b);event(4);return X(3);}
typedef int (*Pair)(const X&,const X&);
Pair select(){event(4);return two;}
int run_call(Hook h){reset(h);X outer(3);int result=two(X(1),X(2));event(4);return result;}
int run_three(Hook h){reset(h);return three(X(1),X(2),X(3));}
int run_by_value(Hook h){reset(h);return by_value(X(1),X(2));}
int run_binary(Hook h){reset(h);X outer(3);return int(X(1))+int(X(2));}
int run_operator(Hook h){reset(h);return X(1)+X(2);}
int run_compare(Hook h){reset(h);return X(1).value<X(2).value;}
int run_conditional(Hook h,int n){reset(h);return two(n&1?X(1):X(2),n&2?X(3):X(1));}
int run_short(Hook h,int n){reset(h);return (n&1?two(X(1),X(2)):0)+(n&2?two(X(2),X(3)):0);}
int run_nested(Hook h){reset(h);return two(X(1),make(X(2),X(3)));}
int run_result(Hook h){reset(h);X result=make(X(1),X(2));inspect(result);event(9);return result.value;}
int run_member(Hook h){reset(h);X receiver(1);return receiver.member(X(2),X(3));}
int run_indirect(Hook h){reset(h);return select()(X(1),X(2));}
int run_assignment(Hook h){reset(h);return (X(1)=X(2)).value;}
int run_explicit_assignment(Hook h){reset(h);return X(1).operator=(X(2)).value;}
int run_index(Hook h){reset(h);return X(1).pointer()[int(X(2))];}
int run_loop(Hook h){
    reset(h);int total=0;
    for(int n=0;n<3;++n){total+=two(n&1?X(1):X(2),X(3));}
    return total;
}
int run_goto(Hook h){
    reset(h);int n=0,total=0;
again:
    total+=two(n&1?X(1):X(2),X(3));
    if(++n<3)goto again;
    return total;
}
int run_receiver(Hook h){reset(h);return X(1).member(X(2),X(3));}
int run_receiver_conditional(Hook h,int n){reset(h);return (n&1?X(1):X(2)).member(n&2?X(2):X(1),X(3));}
int run_receiver_nested(Hook h){reset(h);return make(X(1),X(2)).member(X(2),X(3));}
struct Receiver {
    X first,second;
    Receiver():first(1),second(2){event(4);}
    ~Receiver() noexcept(false){event(5);}
    X produce(const X& a,const X& b){inspect(a);inspect(b);event(9);return X(3);}
};
int run_receiver_result(Hook h){reset(h);X result=Receiver().produce(X(1),X(2));inspect(result);event(4);return result.value;}
struct Bundle {X a,b;};
int run_receiver_aggregate(Hook h){reset(h);return Bundle{X(1),X(2)}.a.member(X(2),X(3));}
int run_receiver_loop(Hook h){
    reset(h);int total=0;
    for(int n=0;n<3;++n)total+=(n&1?X(1):X(2)).member(X(2),X(3));
    return total;
}
int run_receiver_goto(Hook h){
    reset(h);int n=0,total=0;
again:
    total+=(n&1?X(1):X(2)).member(X(2),X(3));
    if(++n<3)goto again;
    return total;
}
