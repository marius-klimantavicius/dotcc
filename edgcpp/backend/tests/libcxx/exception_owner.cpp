#include <exception>
#include <utility>

using Observer=void(*)(int,void*);
static Observer observer;
static void* observer_state;
struct Guard {
  int alive=1;
  ~Guard(){if(observer)observer(-900,observer_state);alive=0;}
};
static Guard guard;
struct Packet {
  int tag;
  explicit Packet(int n):tag(n){}
  virtual ~Packet(){if(observer)observer(tag+1000*guard.alive,observer_state);}
};
struct Holder {std::exception_ptr value;Holder* next=nullptr;};
static std::exception_ptr saved;
static Holder* owned;
struct HeapCleanup {
  ~HeapCleanup(){while(owned){Holder* p=owned;owned=p->next;delete p;}}
};
static HeapCleanup heap_cleanup;
static thread_local std::exception_ptr tls_saved;

void set_observer(Observer fn,void* state){observer=fn;observer_state=state;}
void seed(int tag){saved=std::make_exception_ptr(Packet(tag));}
void clear_saved(){saved=nullptr;}
void* saved_address(){return &saved;}
void copy_saved(void* source){saved=*static_cast<std::exception_ptr*>(source);}
void copy_to_owned_heap(void* source){auto* p=new Holder;p->value=*static_cast<std::exception_ptr*>(source);p->next=owned;owned=p;}
void copy_to_leaked_heap(){auto* p=new Holder;p->value=saved;}
void transfer_saved(){
  auto local=std::move(saved);saved=nullptr;
  std::swap(local,saved);local=saved;local=local;
  saved=nullptr;saved=std::move(local);
}
void capture_nested(){
  try {
    try {throw Packet(22);}catch(...) {std::throw_with_nested(Packet(21));}
  }catch(...) {saved=std::current_exception();}
}
void raise_saved(){std::rethrow_exception(saved);}
void fill_tls(int tag){tls_saved=std::make_exception_ptr(Packet(tag));}
