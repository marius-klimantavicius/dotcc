//type:fn
//options:--c++11:--c++17
//options_all:-A

struct S {
 S();
 S(S const&) = delete;
private:
 friend void g();
 ~S();
};

S f() {
 return S();
}

void g() {
 S s = f();
}
