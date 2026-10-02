//type:fp
//options:--clang --ms_extensions
//options_all:-W

/* Generate a test case for the specified __ prefixed ms extension */
#define TEST_EXT(ext_name) namespace ms_ext_##ext_name { \
  void test(int __##ext_name) { } \
}

/* These are not implemented as keywords in Clang and should not cause problems
   when used via TEST_EXT. */

/* The following are not implemented in Clang via a keyword (but are
   implemented by us via a keyword).  In the future these should be uncommented
   to verify that our approach conforms to clang's, and does not treat these as
   keywords.

   TEST_EXT(assume)
   TEST_EXT(noop)
   TEST_EXT(except)
   TEST_EXT(w64)
   TEST_EXT(int16)
   TEST_EXT(int64)
   TEST_EXT(unaligned)
   TEST_EXT(LPREFIX)
   TEST_EXT(lPREFIX)
   TEST_EXT(UPREFIX)
   TEST_EXT(uPREFIX) */

TEST_EXT(clrcall)
TEST_EXT(event)
