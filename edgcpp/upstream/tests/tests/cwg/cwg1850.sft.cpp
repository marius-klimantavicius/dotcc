//type:fn
//options_all:--c++17 -tused -A

int j;
template<class T> class X {
void f(T t, int i, char* p) {
t = i; // diagnosed if X::f is instantiated, and the assignment to t is an error
p = i; // may be diagnosed even if X::f is not instantiated
p = j; // may be diagnosed even if X::f is not instantiated
X<T>::g(t);  // added with P178R6
X<T>::h(t):  // added with P178R6 maybe diagnosed even if X::f is not instantiated
}
void g(T t) {
+; // may be diagnosed even if X::g is not instantiated
}
};

//cwg: 1850
//title: Differences between definition context and point of instantiation
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
