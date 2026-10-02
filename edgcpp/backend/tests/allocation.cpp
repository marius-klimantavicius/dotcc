#include <new>
unsigned long long alloc_trace;
int alloc_live,constructs,fail_at,throw_dtor,handler_calls;
void event(int n){alloc_trace=alloc_trace*131+(unsigned)n;}
struct Item {
 int n;Item():n(++constructs){event(n);if(n==fail_at)throw 17;++alloc_live;}
 ~Item() noexcept(false){event(n+100);--alloc_live;if(n==throw_dtor)throw 23;}
};
struct alignas(128) Wide {long n;Wide():n(71){++alloc_live;}~Wide(){--alloc_live;}};
void handler(){++handler_calls;std::set_new_handler(nullptr);}
void bad_handler(){++handler_calls;throw std::bad_alloc();}
void throwing_handler(){++handler_calls;throw 31;}
void* allocate_object(){return new int(41);}
void free_object(void* p){delete (int*)p;}
int read_alloc_live(){return alloc_live;}
unsigned long long read_alloc_trace(){return alloc_trace;}
int run_alloc(int which,void(*hook)()) {
 alloc_trace=0;constructs=0;fail_at=0;throw_dtor=0;handler_calls=0;
 if(which==0){Item* p=new Item[4];hook();int n=p[3].n;delete[] p;return n;}
 if(which==1){fail_at=3;try{new Item[5];}catch(int n){return n;}return -1;}
 if(which==2){Item* p=new Item[4];throw_dtor=3;try{delete[] p;}catch(int n){return n;}return -1;}
 if(which==3){Item* p=new Item[0];int n=p!=nullptr;delete[] p;return n;}
 if(which==4){Wide* p=new Wide;hook();int n=((unsigned long)p%128==0)+p->n;delete p;return n;}
 if(which==5){Wide* p=new Wide[3];int n=((unsigned long)p%128==0)+p[2].n;delete[] p;return n;}
 if(which==6){void* p=operator new(0);void* q=operator new[](0);int n=p && q && p!=q;operator delete(p);operator delete[](q);return n;}
 if(which==7){void* p=operator new(~0UL,std::nothrow);return p==nullptr;}
 if(which==8){std::set_new_handler(handler);try{operator new(~0UL);}catch(std::bad_alloc&){return handler_calls==1 && !std::get_new_handler();}return -1;}
 if(which==9){std::set_new_handler(throwing_handler);int n=0;try{operator new(~0UL);}catch(int v){n=v;}std::set_new_handler(nullptr);return n+handler_calls;}
 if(which==10){std::set_new_handler(bad_handler);void* p=operator new(~0UL,std::nothrow);std::set_new_handler(nullptr);return !p && handler_calls==1;}
 if(which==11){alignas(Item) unsigned char memory[sizeof(Item)];Item* p=new(memory)Item;int n=(void*)p==(void*)memory;p->~Item();return n;}
 if(which==12){unsigned long count=~0UL;try{new Item[count];}catch(std::bad_array_new_length&){return 1;}return -1;}
 if(which==13){unsigned long count=~0UL;try{Item* p=new(std::nothrow)Item[count];return p==nullptr;}catch(std::bad_array_new_length&){return 2;}}
 if(which==14){Wide* p=new(std::nothrow)Wide[2];int n=p && (unsigned long)p%128==0;delete[] p;return n;}
 if(which==15){int* p=new int[7]();int n=0;for(int i=0;i<7;++i)n+=p[i];delete[] p;return n;}
 if(which==16){Item* p=nullptr;delete[] p;return 1;}
 if(which==17){void* p=operator new[](513,std::align_val_t(256));int n=(unsigned long)p%256==0;operator delete[](p,513,std::align_val_t(256));return n;}
 return -99;
}
