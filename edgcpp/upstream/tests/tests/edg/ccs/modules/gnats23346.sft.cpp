//type:fp
//options:--c++17:--c++17 -DNEG;fn:--c++20:--c++20 -DNEG;fn:--modules:--modules -DNEG;fn

int module;
#if NEG
int export;
#endif
int import;
