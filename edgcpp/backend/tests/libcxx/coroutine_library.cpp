#include <coroutine>
#include <cstdlib>
static int total,live_promises,owner_id;
using Notify=void(*)(int);
static Notify notify;
struct Task {
  struct promise_type;using Handle=std::coroutine_handle<promise_type>;Handle handle;
  struct promise_type {
    int value;
    static void* operator new(decltype(sizeof(0)) size){return std::malloc(size);}
    static void operator delete(void* p)noexcept{std::free(p);}
    promise_type(int input):value(input){++live_promises;}
    ~promise_type(){--live_promises;if(notify)notify(owner_id*1000+value);}
    Task get_return_object(){return Task{Handle::from_promise(*this)};}
    std::suspend_always initial_suspend()noexcept{return {};}
    std::suspend_always final_suspend()noexcept{return {};}
    void return_void()noexcept{}
    void unhandled_exception()noexcept{total=-999;}
  };
};
extern "C" Task create_task(int value){total+=value;co_await std::suspend_always{};total+=value*10;co_return;}
struct OwnedTask {
  Task task;
  ~OwnedTask(){if(task.handle)task.handle.destroy();}
};
#ifdef NATIVE_LIBRARY_ORACLE
static int events[8],event_count,verify_final;
static void native_notify(int value){events[event_count++]=value;}
struct FinalCheck {
  ~FinalCheck(){if(verify_final && (event_count!=3 || events[2]!=1007))std::abort();}
};
static FinalCheck final_check;
#endif
static OwnedTask global_task{create_task(7)};
extern "C" void set_observer(int id,Notify callback){owner_id=id;notify=callback;}
extern "C" int total_value(){return total;}
extern "C" int live_count(){return live_promises;}
extern "C" int resume_task(Task* task){task->handle.resume();return task->handle.done()?1:0;}
extern "C" void destroy_task(Task* task){task->handle.destroy();task->handle=nullptr;}
#ifdef NATIVE_LIBRARY_ORACLE
int main(){
  set_observer(1,&native_notify);if(live_count()!=1 || total_value()!=0)return 1;
  auto task=create_task(3);if(live_count()!=2)return 2;
  if(resume_task(&task)!=0 || total_value()!=3)return 3;
  if(resume_task(&task)!=1 || total_value()!=33)return 4;
  destroy_task(&task);if(live_count()!=1 || event_count!=1 || events[0]!=1003)return 5;
  auto early=create_task(5);destroy_task(&early);
  if(total_value()!=33 || event_count!=2 || events[1]!=1005)return 6;
  verify_final=1;return 0;
}
#endif
