//type:fn
//options:--clang --ms_extensions
//options_all:-W

/* Generate a test case for the specified __ prefixed ms extension */
#define TEST_EXT(ext_name) namespace ms_ext_##ext_name { \
  void test(int __##ext_name) { } \
}

/* These are implemented as keywords in Clang and should cause problems when
   used via TEST_EXT. */

/* Valid in Clang, implemented by their -fms-extensions: */
TEST_EXT(try)
TEST_EXT(finally)
TEST_EXT(leave)
TEST_EXT(cdecl)
TEST_EXT(declspec)
TEST_EXT(fastcall)
TEST_EXT(stdcall)
TEST_EXT(thiscall)
TEST_EXT(vectorcall)
TEST_EXT(forceinline)
TEST_EXT(builtin_alignof)
TEST_EXT(FUNCSIG__)
TEST_EXT(FUNCDNAME__)
TEST_EXT(int8)
TEST_EXT(int32)
TEST_EXT(ptr32)
TEST_EXT(ptr64)
TEST_EXT(sptr)
TEST_EXT(uptr)
TEST_EXT(uuidof)
TEST_EXT(if_exists)
TEST_EXT(if_not_exists)
TEST_EXT(super)
TEST_EXT(interface)
TEST_EXT(identifier)
TEST_EXT(nullptr)
