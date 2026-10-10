template<class T> struct Argument { T value; };
static long seen;
void consume(Argument<long> value) { seen=value.value; }
using Callback=void(*)(Argument<long>);
Callback get_callback();
int main() { get_callback()(Argument<long>{42}); return seen==42?0:1; }
