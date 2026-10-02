// Shared input for native C++ and the managed exception-injection host.
typedef void (*Hook)(int);
unsigned long long trace;
void event(int n) { trace=trace*10+static_cast<unsigned>(n); }
void reset(){trace=0;}
unsigned long long read_trace(){return trace;}
struct X {
    int id;
    X(int n,Hook hook):id(n) {event(n); hook(n);}
    ~X(){event(id+5);}
};
void nested(Hook hook,int early) {
    X a(1,hook);
    {X b(2,hook); if(early) return; hook(9);}
}
struct Y {
    X first,second;
    Y(Hook hook):first(1,hook),second(2,hook){event(3);hook(3);}
    ~Y(){event(8);}
};
void members(Hook hook){Y y(hook);hook(9);}
void loops(Hook hook) {
    for(int i=0;i<3;++i) {X a(1,hook); if(i==0)continue; if(i==1)break;}
}
void jump(Hook hook) {
    X a(1,hook);
    {X b(2,hook);goto done;}
  done: hook(9);
}
void reverse_jump(Hook hook) {
    int n=0;
  again:
    X a(1,hook);
    if(++n<2)goto again;
}
void no_throw(Hook hook) noexcept {X a(1,hook);hook(9);}
struct ThrowingDtor {
    Hook hook;
    ThrowingDtor(Hook h):hook(h){event(4);}
    ~ThrowingDtor() noexcept(false) {event(8);hook(8);}
};
void throwing_dtor(Hook hook) { X a(1,hook); ThrowingDtor b(hook); hook(9); }
