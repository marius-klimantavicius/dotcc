extern "C" int printf(const char*, ...);
static int trace, cleanup;
struct Guard { ~Guard() { ++cleanup; } };
struct Box {
    int value;
    Box& operator=(int x) { trace=trace*10+3; value=x; return *this; }
    Box& operator+=(int x) { trace=trace*10+4; value+=x; return *this; }
};
static Box box;
static Box& receiver() { trace=trace*10+1; return box; }
static int source(bool fail=false) { Guard guard;trace=trace*10+2;if(fail)throw 7;return 5; }
static void show() { printf("%d:%d:%d\n",trace,box.value,cleanup);trace=cleanup=0; }
template<class T> struct Owner {
    T* pointer;
    explicit Owner(T* p) : pointer(p) {}
    Owner& operator=(Owner&& other) { pointer=other.pointer;other.pointer=0;trace=trace*10+6;return *this; }
    ~Owner() { ++cleanup; }
};
template<class T> void replace(Owner<T>& owner,T* payload) { owner=Owner<T>(payload); }
int main() {
    box=9;show();
    box+=2;show();
    receiver()=source();show();
    receiver().operator=(source());show();
    receiver()+=source();show();
    receiver().operator+=(source());show();
    try { receiver()=source(true); } catch(int x) { if(x!=7)return 1; } show();
    try { receiver().operator=(source(true)); } catch(int x) { if(x!=7)return 2; } show();
    int answer=(receiver()=source()).value; if(answer!=5)return 3;show();
    int payload=8;Owner<int> owner(0);replace(owner,&payload);if(*owner.pointer!=8)return 4;show();
    return 0;
}
