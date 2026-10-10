// Constructor rollback crosses anonymous union parent objects omitted in source initializer syntax.
int events;
struct Element {
  Element(int value) { events += value; if(value==3)throw value; }
  ~Element() { events += 10; }
};
template<class T> struct NoDestroy {
  int prefix=123;
  union { union { T object; }; char dummy; };
  NoDestroy(int value):object(value) { if(value<0)throw value; }
  ~NoDestroy() {}
};
int main() {
  { NoDestroy<Element> retained(2); }
  try { NoDestroy<Element> failed(-1); } catch(int value) { if(value!=-1)return 2; }
  try { NoDestroy<Element> failed(3); } catch(int value) { if(value!=3)return 3; }
  return events==14?0:1;
}
