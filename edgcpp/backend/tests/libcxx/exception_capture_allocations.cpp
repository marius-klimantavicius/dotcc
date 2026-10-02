#include "exception_foreign_owner.cpp"

long capture_allocations(int iterations,long(*allocated)()) {
  try {throw Packet(83);}
  catch(...) {
    for(int i=0;i<128;++i){auto a=std::current_exception();auto b=a;a=b;}
    long before=allocated();
    for(int i=0;i<iterations;++i){auto a=std::current_exception();auto b=a;a=b;if(!a || a!=b)return -1;}
    return allocated()-before;
  }
}
