typedef void (*Hook)(int);
Hook active_hook;
unsigned long long trace;
void event(int n){trace=trace*10+static_cast<unsigned>(n);active_hook(n);}
void reset(Hook h){trace=0;active_hook=h;}
unsigned long long read_trace(){return trace;}
struct X {
    int value; const X* self;
    X(int n):value(n),self(this){event(n);}
    X(const X& x):value(x.value),self(this){event(3);}
    X(X&& x):value(x.value),self(this){event(4);}
    ~X() noexcept(false){if(self!=this)event(0);event(value+5);}
};
void inspect(const X& x){if(x.self!=&x)event(0);}
struct Guard {~Guard() noexcept(false){event(9);}};
X named(){X x(1);event(5);return x;}
X direct(){return X(2);}
X forwarded(){return direct();}
X listed(){return {1};}
X moved(){X x(1);return static_cast<X&&>(x);}
X copied(const X& source){return source;}
X chosen(int n){X a(1),b(2);if(n)return a;return b;}
X exit_direct(){Guard g;return X(1);}
X exit_named(){X result(1);Guard g;return result;}
X early_guard(){Guard g;X result(1);return result;}
struct Maker{int value;X make(){return X(value);}};
struct Immovable {
    const Immovable* self;
    Immovable():self(this){event(1);}
    Immovable(const Immovable&)=delete;
    Immovable(Immovable&&)=delete;
    ~Immovable() noexcept(false){event(6);}
};
Immovable guaranteed(){return Immovable();}
X parameter(X value){return value;}
int run_named(Hook h){reset(h);X x=named();inspect(x);event(8);return x.value;}
int run_direct(Hook h){reset(h);X x=direct();inspect(x);event(8);return x.value;}
int run_forwarded(Hook h){reset(h);X x=forwarded();inspect(x);return x.value;}
int run_listed(Hook h){reset(h);X x=listed();inspect(x);return x.value;}
int run_moved(Hook h){reset(h);X x=moved();inspect(x);return x.value;}
int run_copied(Hook h){reset(h);X source(1);X x=copied(source);inspect(x);return x.value;}
int run_chosen(Hook h,int n){reset(h);X x=chosen(n);inspect(x);return x.value;}
int run_exit_direct(Hook h){reset(h);X x=exit_direct();inspect(x);return x.value;}
int run_exit_named(Hook h){reset(h);X x=exit_named();inspect(x);return x.value;}
int run_early_guard(Hook h){reset(h);X x=early_guard();inspect(x);return x.value;}
int run_member(Hook h){reset(h);Maker maker={2};X x=maker.make();inspect(x);return x.value;}
int run_indirect(Hook h){reset(h);X (*factory)()=direct;X x=factory();inspect(x);return x.value;}
int run_guaranteed(Hook h){reset(h);Immovable x=guaranteed();if(x.self!=&x)event(0);return 1;}
int run_parameter(Hook h){reset(h);X x=parameter(X(1));inspect(x);return x.value;}
int run_parameter_loop(Hook h){
    reset(h);int total=0;
    for(int n=0;n<3;++n){X x=parameter(X(1));inspect(x);total+=x.value;}
    return total;
}
int run_parameter_goto(Hook h){
    reset(h);int n=0;
again:
    {X x=parameter(X(1));inspect(x);}
    if(++n<3)goto again;
    return n;
}
int run_nested_parameter(Hook h){reset(h);X x=parameter(parameter(X(1)));inspect(x);return x.value;}
int run_two_parameters(Hook h){
    reset(h);X a=parameter(X(1)),b=parameter(X(2));inspect(a);inspect(b);return a.value+b.value;
}
int run_many_parameters(Hook h){
    reset(h);
    X x0=parameter(X(1));inspect(x0);
    X x1=parameter(X(1));inspect(x1);
    X x2=parameter(X(1));inspect(x2);
    X x3=parameter(X(1));inspect(x3);
    X x4=parameter(X(1));inspect(x4);
    X x5=parameter(X(1));inspect(x5);
    X x6=parameter(X(1));inspect(x6);
    X x7=parameter(X(1));inspect(x7);
    X x8=parameter(X(1));inspect(x8);
    X x9=parameter(X(1));inspect(x9);
    X x10=parameter(X(1));inspect(x10);
    X x11=parameter(X(1));inspect(x11);
    X x12=parameter(X(1));inspect(x12);
    X x13=parameter(X(1));inspect(x13);
    X x14=parameter(X(1));inspect(x14);
    X x15=parameter(X(1));inspect(x15);
    X x16=parameter(X(1));inspect(x16);
    X x17=parameter(X(1));inspect(x17);
    X x18=parameter(X(1));inspect(x18);
    X x19=parameter(X(1));inspect(x19);
    X x20=parameter(X(1));inspect(x20);
    X x21=parameter(X(1));inspect(x21);
    X x22=parameter(X(1));inspect(x22);
    X x23=parameter(X(1));inspect(x23);
    X x24=parameter(X(1));inspect(x24);
    X x25=parameter(X(1));inspect(x25);
    X x26=parameter(X(1));inspect(x26);
    X x27=parameter(X(1));inspect(x27);
    X x28=parameter(X(1));inspect(x28);
    X x29=parameter(X(1));inspect(x29);
    X x30=parameter(X(1));inspect(x30);
    X x31=parameter(X(1));inspect(x31);
    X x32=parameter(X(1));inspect(x32);
    X x33=parameter(X(1));inspect(x33);
    return x0.value+x33.value;
}
