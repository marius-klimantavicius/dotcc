#include <exception>
static int bad,mode,original_live,replacement_live,event_count;
static int events[32];
static std::exception_ptr original_handle,replacement_handle;
static void check(bool condition){if(!condition)++bad;}
static void event(int value){events[event_count++]=value+100*std::uncaught_exception();}
struct Original {
  int value=41;
  Original(){++original_live;}
  Original(const Original& other):value(other.value){++original_live;}
  ~Original(){if(--original_live==0)event(11);}
};
struct Replacement {
  int value=42;
  Replacement(){++replacement_live;}
  Replacement(const Replacement& other):value(other.value){++replacement_live;}
  ~Replacement(){if(--replacement_live==0)event(22);}
};
static void handler() {
  check(original_live==1 && std::uncaught_exception()==0);
  auto original=std::current_exception();check(!!original);
  try{std::rethrow_exception(original);}catch(const Original& value){check(value.value==41);}
  if(mode!=1)original_handle=original;
  throw Replacement();
}
static void replace() throw(Replacement){throw Original();}
static void substitute_bad() throw(std::bad_exception){throw Original();}
extern "C" int dynamic_lifetime_cases() {
  bad=event_count=0;auto previous=std::set_unexpected(handler);
  mode=0;
  try{replace();}catch(const Replacement& value){
    check(value.value==42 && original_live==1 && replacement_live==1);
    replacement_handle=std::current_exception();
  }
  check(original_live==1 && replacement_live==1 && event_count==0);
  original_handle={};check(original_live==0 && replacement_live==1 && event_count==1 && events[0]==11);
  replacement_handle={};check(replacement_live==0 && event_count==2 && events[1]==22);
  mode=1;event_count=0;
  try{replace();}catch(const Replacement& value){
    check(value.value==42 && original_live==0 && replacement_live==1);
    check(event_count==1 && events[0]==111);
  }
  check(replacement_live==0 && event_count==2 && events[1]==22);
  mode=2;event_count=0;
  try{substitute_bad();}catch(const std::bad_exception&){
    check(original_live==1 && replacement_live==0 && event_count==1 && events[0]==122);
    check(!!std::current_exception() && std::uncaught_exception()==0);
  }
  try{std::rethrow_exception(original_handle);}catch(const Original& value){check(value.value==41);}
  original_handle={};check(original_live==0 && event_count==2 && events[1]==11);
  std::set_unexpected(previous);return bad;
}
int main(){return dynamic_lifetime_cases();}
