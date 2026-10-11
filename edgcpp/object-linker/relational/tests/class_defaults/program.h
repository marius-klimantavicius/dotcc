enum class Kind { One = 1, Two = 2 };
extern int alive;
extern int destroyed;
struct Resource { int value; Resource(Kind kind); ~Resource(); };
struct Gate { Gate(int fail); ~Gate(); };
struct Defaulted {
    Resource first = Kind::One;
    Resource second = Kind::Two;
    Gate last;
    Defaulted(int fail);
    Defaulted();
    ~Defaulted();
};
struct ExitLocal { ~ExitLocal() noexcept(false); };
struct ExitFailure { Resource resource; ExitFailure(bool early); };
