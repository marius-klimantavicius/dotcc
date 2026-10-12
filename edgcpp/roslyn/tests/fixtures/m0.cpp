#define INC(v) ((v)+1)
struct Base { int x; virtual ~Base(); virtual int get() const; };
Base::~Base() {}
int Base::get() const { return x; }
struct Derived : virtual Base { int y; ~Derived(); int get() const override; };
Derived::~Derived() {}
int Derived::get() const { return INC(y); }
template<class T> struct Box { T value; T get() const { return value; } };
int use(int n) { Box<int> b = { n }; Derived d; try { if(n) throw n; } catch(int e) { return b.get()+e; } return d.get(); }
int indirect(Base *p) { return p->get(); }
