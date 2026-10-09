// Pending owned source-EH acceptance. No standard-library dependencies.
static unsigned long long trace;
struct Track {
    int digit;
    explicit Track(int value) : digit(value) { trace = trace * 10 + digit; }
    ~Track() noexcept(false) { trace = trace * 10 + digit; }
};
static void rethrow_active() { throw; }
enum FirstCode { first_code = 17 };
enum SecondCode { second_code = 17 };
static void nonthrowing_target() noexcept {}
using Function = void (*)();
using NoexceptFunction = void (*)() noexcept;
struct Prefix { int prefix; };
struct Base { int value; };
struct PublicDerived : Prefix, Base {
    PublicDerived() { prefix = 13; value = 29; }
};
struct PrivateDerived : private Base { PrivateDerived() { value = 31; } };
struct Left : Base {};
struct Right : Base {};
struct Ambiguous : Left, Right {};
static int copies, destructions;
struct Payload {
    int value;
    explicit Payload(int input) : value(input) {}
    Payload(const Payload &other) : value(other.value) { ++copies; }
    ~Payload() noexcept(false) { ++destructions; }
};
static void throw_lvalue(const Payload &value) { throw value; }
extern "C" int source_eh_test() {
    trace = 0;
    {
        Track outer(1);
        try { Track inner(2); throw 7; }
        catch (int value) {
            if (value != 7 || trace != 122) return 1;
            trace = trace * 10 + 3;
        }
        if (trace != 1223) return 2;
    }
    if (trace != 12231) return 3;
    trace = 0;
    try {
        Track outer(1);
        try { Track inner(2); throw 5; }
        catch (int value) { if (value != 5) return 4; trace = trace * 10 + 3; rethrow_active(); }
    } catch (int value) {
        if (value != 5 || trace != 12231) return 5;
        trace = trace * 10 + 4;
    }
    if (trace != 122314) return 6;
    int first = 11, second = 22;
    try { throw &first; }
    catch (int *view) {
        if (view != &first) return 7;
        view = &second;
        try { rethrow_active(); }
        catch (int *original) { if (original != &first || view != &second) return 8; }
        catch (...) { return 9; }
    }
    try { throw &first; }
    catch (const int *view) { if (view != &first || *view != 11) return 10; }
    catch (...) { return 11; }
    try { throw (long)17; }
    catch (long long) { return 12; }
    catch (long value) { if (value != 17) return 13; }
    catch (...) { return 14; }
    try { throw first_code; }
    catch (SecondCode) { return 15; }
    catch (FirstCode value) { if (value != first_code) return 16; }
    catch (...) { return 17; }
    try { throw (NoexceptFunction)nonthrowing_target; }
    catch (Function function) { if (function != nonthrowing_target) return 18; function(); }
    catch (...) { return 19; }
    try { throw PublicDerived(); }
    catch (const Base &base) { if (base.value != 29) return 20; }
    catch (...) { return 21; }
    try { throw PrivateDerived(); }
    catch (const Base &) { return 22; }
    catch (const PrivateDerived &) {}
    catch (...) { return 23; }
    try { throw Ambiguous(); }
    catch (const Base &) { return 24; }
    catch (const Ambiguous &) {}
    catch (...) { return 25; }
    copies = destructions = 0;
    {
        Payload original(37);
        try { throw_lvalue(original); }
        catch (const Payload &caught) {
            if (caught.value != 37 || copies != 1 || destructions != 0) return 26;
        }
        if (copies != 1 || destructions != 1) return 27;
    }
    if (destructions != 2) return 28;
    try { throw 41; }
    catch (...) {}
    return 0;
}
int main() { return source_eh_test(); }
