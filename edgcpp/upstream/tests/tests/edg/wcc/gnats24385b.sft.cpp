//type:fn
//options:--g++ --ms_extensions
//options_all:-W

/* Generate a test case for the specified __ prefixed ms extension */
#define TEST_EXT(ext_name) namespace ms_ext_##ext_name { \
  void test(int __##ext_name) { } \
}

/* These are implemented as keywords in GCC and should cause problems when
   used via TEST_EXT. */

/* Valid in GCC because they're the same keywords: */
TEST_EXT(alignof)
TEST_EXT(inline)
