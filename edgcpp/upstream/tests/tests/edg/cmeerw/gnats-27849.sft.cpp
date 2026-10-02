//type:fp
//options:--target linux_armv7:--target linux_aarch64:--target linux_armv7 -DERROR;fn:--target linux_aarch64 -DERROR;fn
//options_all:-w --ms_c++20 --microsoft_version 1928

void f()
{
  { __assume(1); }
  { __annotation(L""); }
  { __builtin_zero_non_value_bits((char *) nullptr); }
  { __debugbreak(); }

  { int v = __builtin_COLUMN(); }
  { const char * v = __builtin_FILE(); }
  { const char * v = __builtin_FUNCTION(); }
  { int v = __builtin_LINE(); }
  { char *v = __builtin_char_memchr("", 0, 0); }
  { __builtin_coro_destroy(nullptr); }
  { __builtin_coro_done(nullptr); }
  { void *v = __builtin_coro_noop(); }
  { void *v = __builtin_coro_promise(nullptr, 0, false); }
  { __builtin_coro_resume(nullptr); }
  { double v = __builtin_huge_val(); }
  { float v = __builtin_huge_valf(); }
  { int v = __builtin_memcmp("", "", 0); }
  { double v = __builtin_nan(""); }
  { float v = __builtin_nanf(""); }
  { double v = __builtin_nans(""); }
  { float v = __builtin_nansf(""); }
  { unsigned long v = __builtin_strlen(""); }
  { unsigned long v =__builtin_wcslen(L""); }
  { wchar_t *v = __builtin_wmemchr(L"", L' ', 0); }
  { int v = __builtin_wmemcmp(L"", L" ", 0); }

#ifdef ERROR
  __builtin_clz(0);
  __builtin_clzl(0);
  __builtin_clzll(0);
  __builtin_ctz(0);
  __builtin_ctzl(0);
  __builtin_ctzll(0);
  __builtin_huge_vall();
  __builtin_memchr("", 0, 0);
  __builtin_nanl("");
  __builtin_nansl("");
  __builtin_popcount(0);
  __builtin_popcountl(0);
  __builtin_popcountll(0);
  __builtin_strchr("", 0);
  __builtin_strcmp("", "");
  __builtin_strncmp("", "", 0);
  __builtin_wcschr(L"", L' ');
  __builtin_wcscmp(L"", L"");
  __builtin_wcsncmp(L"", L"", 0);
#endif
}
