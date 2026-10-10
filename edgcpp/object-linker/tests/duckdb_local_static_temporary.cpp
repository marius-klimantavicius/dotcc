struct Tag{};
int called; int touch(){++called;return 7;}
struct Payload {int value; Payload(Tag,int x):value(x){}};
template<class T> struct Box {union { T value; char dummy; }; ~Box(){} template<class...A> Box(A&&...a):value(static_cast<A&&>(a)...){}};
int get(){static const Box<Payload> box(Tag{},[]{return touch();}());return box.value.value;}
int other(){static int value=touch();return value;}
int main(){return get()!=7 || get()!=7 || called!=1;}
