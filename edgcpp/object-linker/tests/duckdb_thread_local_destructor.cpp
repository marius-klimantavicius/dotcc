extern "C" [[noreturn]] void abort();
int destroyed_count;
int expected_count=1;
struct FinalCheck { ~FinalCheck(){if(destroyed_count!=expected_count)abort();} } final_check;
thread_local int attempts;
struct Value {
    int value;
    Value(){if(++attempts==1)throw 19;value=73;}
    ~Value(){++destroyed_count;}
};
Value &cached(){static thread_local Value value;return value;}
extern "C" void set_expected(int count){expected_count=count;}
extern "C" int check_destroyed(int count){return destroyed_count==count?0:5;}
int main(){
    try{(void)cached();return 1;}catch(int error){if(error!=19)return 2;}
    Value *first=&cached();
    if(first->value!=73||attempts!=2)return 3;
    first->value=81;
    if(&cached()!=first||cached().value!=81||attempts!=2)return 4;
    return 0;
}
