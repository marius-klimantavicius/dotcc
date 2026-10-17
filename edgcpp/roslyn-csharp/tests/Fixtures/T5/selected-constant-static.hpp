#ifndef SELECTED_CONSTANT_STATIC_HPP
#define SELECTED_CONSTANT_STATIC_HPP

inline const char* selected_shared_constant() noexcept
{
    static const char shared_bytes[50] = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLM";
    return shared_bytes;
}

inline const char* selected_different_constant() noexcept
{
    static const char different_bytes[50] = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLM";
    return different_bytes;
}

extern "C" const char* selected_constant_a() noexcept;
extern "C" const char* selected_constant_b() noexcept;
extern "C" const char* selected_different_a() noexcept;
extern "C" const char* selected_different_b() noexcept;

#endif
