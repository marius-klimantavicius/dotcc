using Size=decltype(sizeof(0));
alignas(16) unsigned char arena[4096];
int allocated,released,attempts,live,destroyed,bad_zero,fail_at;
void *allocate_array(Size size) { ++allocated; if(size>sizeof(arena))throw 99; for(Size i=0;i<size;++i)arena[i]=0xa5; return arena; }
void release_array(void *p) noexcept { if(p==arena)++released; }

struct Node {int value;};
struct Counter {
    Counter(){if(++attempts==fail_at)throw 7;++live;}
    ~Counter(){--live;}
};
template<class T>struct Owner {
    static void *operator new[](Size size){return allocate_array(size);}
    static void operator delete[](void *p,Size) noexcept {release_array(p);}
    int T::*member;
    T *pointer;
    Counter counter;
    Owner()=default;
    ~Owner(){if(member!=nullptr || pointer!=nullptr)++bad_zero;++destroyed;}
};
Owner<Node>*make_array(Size n){return new Owner<Node>[n]();}
int main(){
    auto p=make_array(3);
    if(live!=3 || allocated!=1 || released)return 1;
    for(int i=0;i<3;++i)if(p[i].member!=nullptr || p[i].pointer!=nullptr)return 2;
    delete[]p;
    if(live || released!=1 || destroyed!=3 || bad_zero)return 3;
    fail_at=attempts+2;
    try{make_array(3);return 4;}catch(int n){if(n!=7)return 5;}
    if(live || released!=2 || allocated!=2 || destroyed!=4 || bad_zero)return 6;
    return 0;
}
