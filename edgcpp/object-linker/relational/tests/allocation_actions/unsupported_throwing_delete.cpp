struct Throwing { ~Throwing() noexcept(false); };
void release_throwing(Throwing* pointer) { delete pointer; }
