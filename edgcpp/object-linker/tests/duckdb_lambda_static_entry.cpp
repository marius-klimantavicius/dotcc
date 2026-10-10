extern "C" void free(void *);
void operator delete(void *p) noexcept { free(p); }
void operator delete(void *p, unsigned long) noexcept { free(p); }
static int results_destroyed;
struct Result {
 int value;
 explicit Result(int v):value(v) {}
 Result(const Result&)=delete;
 ~Result() { ++results_destroyed; }
};
struct Argument {
 int value;
 explicit Argument(int v):value(v) {}
 Argument(const Argument &other):value(other.value) {}
 ~Argument() {}
};
using Callback = int (*)(int);
using NoexceptCallback = int (*)(int) noexcept;
using ResultCallback = Result (*)(int);
using ArgumentCallback = int (*)(Argument);
Callback make_callback() { return [](int value) { if(value<0)throw 17; return value+3; }; }
Callback make_generic() { return [](auto value) { return value*2; }; }
NoexceptCallback make_noexcept() { return [](int value) noexcept { return value-2; }; }
ResultCallback make_result() { return [](int value) { return Result(value+5); }; }
ArgumentCallback make_argument() { return [](Argument value) { return value.value+9; }; }
int main() {
 Callback callback=make_callback();
 if(callback(4)!=7 || make_generic()(6)!=12 || make_noexcept()(8)!=6)return 1;
 try { callback(-1); return 2; } catch(int code) { if(code!=17)return 3; }
 { Result value=make_result()(10); if(value.value!=15)return 4; }
 if(results_destroyed!=1 || make_argument()(Argument(11))!=20)return 5;
 return 0;
}
