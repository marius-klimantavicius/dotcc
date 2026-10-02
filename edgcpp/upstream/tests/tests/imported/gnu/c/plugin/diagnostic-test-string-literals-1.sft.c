//type: fn
//options: 
# 0 "./plugin/diagnostic-test-string-literals-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./plugin/diagnostic-test-string-literals-1.c"
# 16 "./plugin/diagnostic-test-string-literals-1.c"
extern void __emit_string_literal_range (const void *literal, int caret_idx,
      int start_idx, int end_idx);

void
test_simple_string_literal (void)
{
  __emit_string_literal_range ("0123456789",
          6, 6, 7);




}

void
test_concatenated_string_literal (void)
{
  __emit_string_literal_range ("01234" "56789",
          4, 3, 6);




}

void
test_multiline_string_literal (void)
{
  __emit_string_literal_range ("01234"
                               "56789",
                               4, 3, 6);







}
# 75 "./plugin/diagnostic-test-string-literals-1.c"
void
test_hex (void)
{



  __emit_string_literal_range ("01234\x35 789",
          4, 3, 7);




}

void
test_oct (void)
{



  __emit_string_literal_range ("01234\065 789",
          4, 3, 7);




}

void
test_multiple (void)
{


  __emit_string_literal_range ("01234" "\x35" "\066" "789",
          5, 3, 8);




}

void
test_ucn4 (void)
{






  __emit_string_literal_range ("01234\u2174\u2175789",
          5, 4, 11);




}

void
test_ucn8 (void)
{




  __emit_string_literal_range ("01234\U00002174\U00002175789",
          5, 4, 11);




}

void
test_u8 (void)
{

  __emit_string_literal_range (u8"0123456789",
          6, 4, 7);




}

void
test_u (void)
{

  __emit_string_literal_range (u"0123456789",
          6, 4, 7);




}

void
test_U (void)
{

  __emit_string_literal_range (U"0123456789",
          6, 4, 7);




}

void
test_L (void)
{

  __emit_string_literal_range (L"0123456789",
          6, 4, 7);




}

void
test_raw_string_one_liner (void)
{

  __emit_string_literal_range (R"foo(0123456789)foo",
          6, 4, 7);




}

void
test_raw_string_multiline (void)
{
  __emit_string_literal_range (R"foo(
hello
world
)foo",
          6, 4, 7);
# 227 "./plugin/diagnostic-test-string-literals-1.c"
}

void
test_macro (void)
{

  __emit_string_literal_range ("01234"
                               "56789",
                               4, 3, 6);
# 244 "./plugin/diagnostic-test-string-literals-1.c"
}

void
test_multitoken_macro (void)
{

  __emit_string_literal_range (("0123456789"), 4, 3, 6);
# 264 "./plugin/diagnostic-test-string-literals-1.c"
}




void
test_terminator_location (void)
{
  __emit_string_literal_range ("0123456789",
          10, 10, 10);




}




void
test_backslash_continued_logical_lines (void)
{
  __emit_string_literal_range ("0123456789",

        6, 6, 7);
# 298 "./plugin/diagnostic-test-string-literals-1.c"
}



# 1 "./plugin/pr87562-a.h" 1
# 303 "./plugin/diagnostic-test-string-literals-1.c" 2




# 1 "./plugin/pr87562-b.h" 1
# 308 "./plugin/diagnostic-test-string-literals-1.c" 2

void
pr87652 (const char *stem, int counter)
{
  char label[100];
  do { __emit_string_literal_range ("*.%s%u", 2, 2, 3); } while (0);







}
# 346 "./plugin/diagnostic-test-string-literals-1.c"
void pr87721 (void) {
  do { __emit_string_literal_range("./plugin/diagnostic-test-string-literals-1.c"":%5d: " "Bad password, expected [%s], got [%s].", __builtin_strlen ("./plugin/diagnostic-test-string-literals-1.c") + __builtin_strlen(":%5d: ") + 24, __builtin_strlen ("./plugin/diagnostic-test-string-literals-1.c") + __builtin_strlen(":%5d: ") + 24, __builtin_strlen ("./plugin/diagnostic-test-string-literals-1.c") + __builtin_strlen(":%5d: ") + 25); } while (0);




}
