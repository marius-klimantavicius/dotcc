//remark:dynamic_cast<void*> diagnostic
//options:--c++17;fn

struct S {};

auto p = dynamic_cast<void*>((S*)nullptr);

