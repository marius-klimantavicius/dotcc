#include <exception>
#include <utility>
#include <cstddef>

static int live, destroyed, bad;
static bool aligned_payload(const void* p) {
#ifdef EDG_NATIVE_ALIGNMENT_OBSERVATION
  // Some installed native runtimes supply only max_align_t exception storage.
  // Source semantic oracle observations retain that limitation separately;
  // generated C# must satisfy this check, including over-aligned payloads.
  (void)p;return true;
#else
  return ((std::size_t)p&63)==0;
#endif
}
struct alignas(64) Packet {
  int value;
  explicit Packet(int x):value(x){++live;}
  Packet(const Packet& p):value(p.value){++live;}
  virtual ~Packet(){if(value<0)++bad;--live;++destroyed;}
};
struct CopyFailure {
  CopyFailure()=default;
  CopyFailure(const CopyFailure&){throw 73;}
};
struct Member {int value=42;int get(int n) const noexcept {return value+n;}};
int main() {
  if(std::current_exception())return 1;
  {
    std::exception_ptr saved;
    try {throw Packet(37);}catch(const Packet& p) {
      if(p.value!=37 || !aligned_payload(&p))return 2;
      saved=std::current_exception();
      if(!saved || saved!=std::current_exception())return 3;
    }
    if(live!=1)return 4;
    auto copy=saved; saved=nullptr;
    for(int i=0;i<3;++i) {
      try {std::rethrow_exception(copy);}catch(Packet& p) {
        if(p.value!=37+i || std::current_exception()!=copy)return 5;
        ++p.value;
      }
    }
    auto moved=std::move(copy); // Pinned libc++ implements this as a copy.
    std::swap(copy,moved);moved=copy;copy=copy;
    saved=std::move(copy);copy=nullptr;moved=nullptr;
    try {std::rethrow_exception(saved);}catch(const Packet& p){if(p.value!=40)return 6;}
  }
  if(live || bad || destroyed==0)return 7;
  {
    auto made=std::make_exception_ptr(Packet(61));
    if(live!=1)return 8;
    try {std::rethrow_exception(made);}catch(const Packet& p) {
      if(p.value!=61 || !aligned_payload(&p) || std::current_exception()!=made)return 9;
    }
  }
  if(live || bad)return 10;
  {
    auto made=std::make_exception_ptr(CopyFailure{});
    try {std::rethrow_exception(made);}catch(int value){if(value!=73)return 11;}
  }
  {
    std::exception_ptr nested;
    try {
      try {throw 91;}catch(...) {std::throw_with_nested(Packet(82));}
    } catch(...) {nested=std::current_exception();}
    try {std::rethrow_exception(nested);}catch(const Packet& p) {
      if(p.value!=82)return 12;
      bool seen=false;try {std::rethrow_if_nested(p);}catch(int value){seen=true;if(value!=91)return 13;}
      if(!seen)return 17;
    }
  }
  if(live || bad)return 14;
  {
    using P=int Member::*;using F=int(Member::*)(int) const noexcept;
    auto data=std::make_exception_ptr(P(&Member::value));
    auto method=std::make_exception_ptr(F(&Member::get));
    Member object;
    try {std::rethrow_exception(data);}catch(P p){if(object.*p!=42)return 15;}
    try {std::rethrow_exception(method);}catch(F p){if((object.*p)(3)!=45)return 16;}
  }
  return 0;
}
