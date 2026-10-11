#include "program.h"
int alive = 0;
int destroyed = 0;
Resource::Resource(Kind kind) : value(static_cast<int>(kind)) { ++alive; }
Resource::~Resource() { --alive; destroyed = destroyed * 10 + value; }
Gate::Gate(int fail) { if (fail) throw 17; }
Gate::~Gate() {}
Defaulted::Defaulted(int fail) : last(fail) {}
Defaulted::Defaulted() : first(Kind::Two), last(0) {}
Defaulted::~Defaulted() {}
ExitLocal::~ExitLocal() noexcept(false) { throw 31; }
ExitFailure::ExitFailure(bool early) : resource(Kind::One) { ExitLocal local; if (early) return; }
