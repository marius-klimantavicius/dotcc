//type:cp
//options:--gnu_version 70100:--gnu_version 70200
//options_all:--c++11

struct Foo { int a; };

template<typename T> struct Bar {
    T baz;
    T fun() { return this->baz = { .a = 42 }; }
};

int main(){
    Bar<Foo> *b = new Bar<Foo>();
    b->fun();
}
