unsigned long long init_trace;
void (*observer)(int);
void event(int n){init_trace=init_trace*131+(unsigned)n;if(observer)observer(n);}
__attribute__((constructor(125))) void early_start(){event(32);}
int produce(){event(1);return 17;}
int number=produce();
struct Item {int n;Item(int v):n(v){event(n);}~Item(){event(n+100);}};
Item first(2), array[2]={{3},{4}};
Item priority_later __attribute__((init_priority(200))) (30);
Item priority_earlier __attribute__((init_priority(150))) (31);
int attempts;
struct Retry {int n;Retry(int v):n(v){event(10);if(++attempts==1)throw 7;event(11);}~Retry(){event(112);}};
int local(int n){static Retry value(n);return value.n;}
thread_local Item thread_item(20);
int tls(){return thread_item.n;}
int tls_local(){thread_local Item value(21);return value.n;}
int trivial_local(){static int value=produce();return ++value;}
int run_init(){int caught=0;try{local(5);}catch(int n){caught=n;}int a=local(6),b=local(9);return number+caught+a+b+tls()+tls_local()+trivial_local()+trivial_local();}
unsigned long long read_init(){return init_trace;}
void observe(void(*p)(int)){observer=p;}
int local_parallel(void(*hook)()){static int value=(hook(),42);return value;}
int enter_hook(void(*hook)()){hook();return 19;}
int (*get_hook())(){return &tls;}
int recursive(){static int value=recursive();return value;}
