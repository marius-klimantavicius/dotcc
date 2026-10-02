//remark:new-type-id rendering
//options:--c++17;fp

typedef unsigned long size_t;
 
void *operator new[](size_t);
 
template< class T>
struct S1 {
int row;
void foo();
};
 
void *ptr;
 
template< class T> void
S1< T> ::foo() {
ptr = (new T [row * row]); 
}
 
 
int main() {
S1< double>  xxx;
xxx.foo(); return 0;
}
