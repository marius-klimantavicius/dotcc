//type:fp
//options:--g++ --ms_extensions
//options_all:-W

/* Generate a test case for the specified __ prefixed ms extension */
#define TEST_EXT(ext_name) namespace ms_ext_##ext_name { \
  void test(int __##ext_name) { } \
}

/* These are not implemented as keywords in GCC and should not cause problems
   when used via TEST_EXT. */

/* The following are not implemented in GCC via a keyword (but are implemented
   by us via a keyword).  In the future these should be uncommented to verify
   that our approach conforms to GCC's, and does not treat these as keywords.

   TEST_EXT(if_exists)
   TEST_EXT(if_not_exists)
   TEST_EXT(declspec) */

TEST_EXT(try)
TEST_EXT(finally)
TEST_EXT(leave)
TEST_EXT(except)
TEST_EXT(cdecl)
TEST_EXT(fastcall)
TEST_EXT(stdcall)
TEST_EXT(thiscall)
TEST_EXT(vectorcall)
TEST_EXT(clrcall)
TEST_EXT(forceinline)
TEST_EXT(unaligned)
TEST_EXT(assume)
TEST_EXT(builtin_alignof)
TEST_EXT(FUNCSIG__)
TEST_EXT(FUNCDNAME__)
TEST_EXT(int8)
TEST_EXT(int16)
TEST_EXT(int32)
TEST_EXT(int64)
TEST_EXT(ptr32)
TEST_EXT(ptr64)
TEST_EXT(sptr)
TEST_EXT(uptr)
TEST_EXT(w64)
TEST_EXT(noop)
TEST_EXT(uuidof)
TEST_EXT(super)
TEST_EXT(interface)
TEST_EXT(event)
TEST_EXT(identifier)
TEST_EXT(LPREFIX)
TEST_EXT(lPREFIX)
TEST_EXT(UPREFIX)
TEST_EXT(uPREFIX)
TEST_EXT(nullptr)
