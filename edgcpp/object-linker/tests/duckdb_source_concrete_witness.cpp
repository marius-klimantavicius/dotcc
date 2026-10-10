// An unused template causes original member capture without making its expression graph executable.
template<class T> struct Box { T value; static T get(T x) { return x; } };
template<class T> concept Has = requires { Box<int>::get(1); };
template<Has T> struct Unused { T value; };
extern "C" int inspect(Box<int>* p) { return p->value; }
int main() { Box<int> value{7}; return inspect(&value) == 7 ? 0 : 1; }
