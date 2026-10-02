int trace;
struct Object {
    int id;
    Object(int n):id(n){trace=trace*10+n;}
    ~Object(){trace=trace*10+id+5;}
};
int work(int early){Object a(1);{Object b(2);if(early)return 3;}return 4;}
int main(){
    if(work(1)!=3 || trace!=1276)return 1;
    trace=0;
    if(work(0)!=4 || trace!=1276)return 2;
    return 0;
}
