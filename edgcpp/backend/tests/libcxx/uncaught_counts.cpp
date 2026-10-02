#include <exception>

static int bad;
static unsigned sequence;
static std::exception_ptr expected_during_copy;
static void observe(int expected,int event) {
  if(std::uncaught_exceptions()!=expected)++bad;
  sequence=sequence*17u+(unsigned)event;
}
struct Guard {
  int event;
  ~Guard() noexcept {
    observe(1,event);
    try {throw 52;}catch(int x) {if(x!=52)++bad;observe(1,event+1);}
  }
};
struct Payload {
  Payload(){observe(0,1);}
  Payload(const Payload&){observe(1,2);}
  ~Payload(){if(std::uncaught_exceptions()<0)++bad;}
};
struct CurrentProbe {
  CurrentProbe()=default;
  CurrentProbe(const CurrentProbe&) {
    if(std::current_exception()!=expected_during_copy)++bad;
    observe(1,20);
  }
};
struct CatchCopy {
  CatchCopy()=default;
  CatchCopy(const CatchCopy&){observe(1,6);}
};
static void rethrow_saved(std::exception_ptr saved) {
  Guard guard{8};std::rethrow_exception(saved);
}
int main() {
  observe(0,0);
  try {Guard guard{3};throw Payload();}
  catch(const Payload&) {observe(0,5);}
  if(bad)return 1;
  try {throw CatchCopy();}catch(CatchCopy) {observe(0,7);}
  if(bad)return 2;
  std::exception_ptr saved;
  try {throw 61;}catch(...) {saved=std::current_exception();observe(0,10);}
  try {rethrow_saved(saved);}catch(int x) {if(x!=61)++bad;observe(0,11);}
  if(bad)return 3;
  try {
    try {throw 73;}catch(...) {Guard guard{12};throw;}
  }catch(int x){if(x!=73)++bad;observe(0,14);}
  if(bad)return 4;
  try {throw CurrentProbe();}catch(CurrentProbe) {
    if(!std::current_exception())++bad;
    observe(0,21);
  }
  if(bad)return 5;
  try {throw 91;}catch(...) {
    expected_during_copy=std::current_exception();
    try {throw CurrentProbe();}catch(CurrentProbe) {
      if(std::current_exception()==expected_during_copy)++bad;
      observe(0,22);
    }
    if(std::current_exception()!=expected_during_copy)++bad;
  }
  expected_during_copy=nullptr;
  if(std::current_exception())++bad;
  return bad?6:0;
}
