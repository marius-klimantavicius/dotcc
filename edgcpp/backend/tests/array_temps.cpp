typedef void (*Hook)(int);
Hook active_hook;
unsigned long long trace;
int next_id;
void record(int n){trace=trace*10+static_cast<unsigned>(n);}
void reset(Hook h){trace=0;next_id=0;active_hook=h;}
unsigned long long read_trace(){return trace;}
struct X {
    int id;
    X():id(++next_id){record(id);active_hook(id);}
    X(int n):id(n){record(id);active_hook(id);}
    ~X() noexcept(false){record(id+5);active_hook(id+5);}
};
void defaults(Hook h){reset(h);X a[3];active_hook(9);}
void braces(Hook h){reset(h);X a[3]={X(1),X(2),X(3)};active_hook(9);}
void multi(Hook h){reset(h);X a[2][2];active_hook(5);}
struct Pack { X elements[3]; Pack(){record(4);active_hook(4);} ~Pack() noexcept(false){record(9);} };
void members(Hook h){reset(h);Pack p;active_hook(5);}
int use(const X& x){active_hook(4);return x.id;}
void single(Hook h){reset(h);use(X(1));active_hook(9);}
void sequence(Hook h){reset(h);(use(X(1)),use(X(2)));active_hook(9);}
void conditional(Hook h,int choose){reset(h);int n=choose?use(X(1)):use(X(2));active_hook(n+5);}
void short_circuit(Hook h,int choose){reset(h);if(choose && use(X(1)))active_hook(9);}
void extended(Hook h){reset(h);const X& r=X(1);active_hook(r.id+8);}
int returning(Hook h){reset(h);return use(X(1));}
int comma_lvalue(Hook h){reset(h);int n=0;(record(4),n)=(record(3),8);return n;}
void temp_loop(Hook h){reset(h);for(int i=0;i<2;++i)use(X(1));}
void multi_brace(Hook h){reset(h);X a[2][2]={{X(1),X(2)},{X(3),X(4)}};active_hook(5);}
struct Empty {~Empty() noexcept(false){record(6);active_hook(6);}};
void use_empty(const Empty&){active_hook(4);}
void empty_array(Hook h){reset(h);Empty a[3];active_hook(4);}
void empty_temp(Hook h){reset(h);use_empty(Empty{});}
