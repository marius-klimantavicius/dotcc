//type:cp
//options_all:--c++20 --clang_version 170000

// This should be the current file's file name without any qualifying path
// information.
static_assert(__builtin_FILE_NAME() == "Test_name.c");
