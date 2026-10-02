//type:fp
//options:--c++17 --clang_version 180100:--ms_c++17 --microsoft_version 1936

static_assert(noexcept(__builtin_wmemcmp(L"", L"", 1)));
