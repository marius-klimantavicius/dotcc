//type:cp
//options:--gnu_version 70100:--gnu_version 70200
//options_all:--c++11
//fixing_pr:22112
struct Foo { int a; };

template<typename T> struct Bar {
    Foo baz;
    Foo fun() { return this->baz = { .a = 42 }; }
};

int main(){
    Bar<int> *b = new Bar<int>();
    b->fun();
}
