int destroyed;
struct Value { int value=3; Value(); Value(const Value&); ~Value(); };
Value::Value() : value(3) {}
Value::Value(const Value &v):value(v.value){}
Value::~Value(){++destroyed;}
struct Base { virtual ~Base(); };
Base::~Base(){}
struct Object : virtual Base { Value first; Value second; Object(); ~Object(); };
Object::Object() {}
Object::~Object() {}
Value &local() { static Value v; return v; }
int objects(int n) { Value values[3]; Object o; Value *p=new Value[4]; try { if(n) throw Value(); } catch(const Value &v) { n+=v.value; } delete []p; return n+local().value+values[1].value; }
