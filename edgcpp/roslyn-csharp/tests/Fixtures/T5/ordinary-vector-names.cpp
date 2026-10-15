// Genuine ordinary declarations do not authorize vector-helper substitution.
// This source has no automatic class array or retained vector cleanup protocol.
extern "C" void __cxa_vec_ctor(void*, unsigned long, unsigned long,
                               void (*)(void*), void (*)(void*));
extern "C" void __cxa_vec_dtor(void*, unsigned long, unsigned long, void (*)(void*));

extern "C" int fixture()
{
    __cxa_vec_ctor(nullptr, 0, sizeof(int), nullptr, nullptr);
    __cxa_vec_dtor(nullptr, 0, sizeof(int), nullptr);
    return 0;
}
