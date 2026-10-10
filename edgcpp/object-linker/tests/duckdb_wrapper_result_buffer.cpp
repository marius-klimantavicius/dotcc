extern "C" void free(void *);
void operator delete(void *p) noexcept { free(p); }
void operator delete(void *p, unsigned long) noexcept { free(p); }
static int constructions, destructions;
struct Result {
 int value;
 explicit Result(int v):value(v) { if(v<0)throw 17; ++constructions; }
 Result(const Result&)=delete;
 ~Result() { ++destructions; }
};
struct First { virtual int first() { return 3; } };
struct Second { virtual Result make(int value) = 0; };
struct Derived : First, Second { Result make(int value) override { return Result(value+7); } };
int call(Second &object, int input) { Result result=object.make(input); return result.value; }
int main() {
 Derived object;
 if(call(object,5)!=12 || constructions!=1 || destructions!=1)return 1;
 try { call(object,-10); return 2; } catch(int code) { if(code!=17)return 3; }
 return constructions==1 && destructions==1?0:4;
}
