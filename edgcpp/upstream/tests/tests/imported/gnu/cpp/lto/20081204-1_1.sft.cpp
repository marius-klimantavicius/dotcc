//type: fp
//options: 
struct Foo { virtual ~Foo(); };
struct Bar:public Foo { Bar() { } };
void Func() { new Bar(); }
