int destroyed = 0;
int throw_member = 0;
struct Plain { int value; Plain(int); ~Plain(); };
struct Holder { Plain value{1}; };
Holder global_holder;
Plain::Plain(int input) : value(input) {}
Plain::~Plain() { ++destroyed; }
struct Member { ~Member() noexcept(false) { if (throw_member) throw 19; } };
struct Throwing { Member member; Throwing(int); ~Throwing(); };
struct ThrowingHolder { Throwing value{2}; };
ThrowingHolder global_throwing;
Throwing::Throwing(int) {}
Throwing::~Throwing() {}
static_assert(noexcept(((Plain*)0)->~Plain()), "implicit nonthrowing destructor");
static_assert(!noexcept(((Throwing*)0)->~Throwing()), "implicit throwing member policy");
int main() {
    { Holder local; }
    if (destroyed != 1) return 1;
    throw_member = 1;
    try { ThrowingHolder local; }
    catch (int value) { throw_member = 0; return value == 19 ? 0 : 2; }
    return 3;
}
