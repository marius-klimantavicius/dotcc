// Extends the ordinary owner fixture with abandonment and cyclic references.
#include "exception_owner.cpp"

void copy_foreign_to_leaked_heap(void* source){auto* p=new Holder;p->value=*static_cast<std::exception_ptr*>(source);}
void wrap_foreign(void* source,int tag){
  try {std::rethrow_exception(*static_cast<std::exception_ptr*>(source));}
  catch(...) {std::throw_with_nested(Packet(tag));}
}
struct Chain {
  std::exception_ptr next;
  int tag;
  explicit Chain(int n):tag(n){}
  ~Chain(){if(observer)observer(tag+1000*guard.alive,observer_state);}
};
void seed_chain(int tag){saved=std::make_exception_ptr(Chain(tag));}
void link_chain(void* source){
  try {std::rethrow_exception(saved);}
  catch(Chain& p){p.next=*static_cast<std::exception_ptr*>(source);}
}
void unlink_chain(){try{std::rethrow_exception(saved);}catch(Chain& p){p.next=nullptr;}}
void overwrite_owned(void* source){owned->value=*static_cast<std::exception_ptr*>(source);}
void swap_owned(){std::swap(owned->value,saved);}
#include <cstdlib>
void* make_foreign_raw_holder(void* source){
  void* memory=std::malloc(sizeof(Holder));
  if(!memory)throw std::bad_alloc();
  auto* holder=new(memory) Holder;
  holder->value=*static_cast<std::exception_ptr*>(source);
  return holder;
}
void release_raw_holder(void* memory){std::free(memory);}
