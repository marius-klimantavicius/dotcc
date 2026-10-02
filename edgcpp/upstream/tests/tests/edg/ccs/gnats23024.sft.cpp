//type:rp
//options_all:--microsoft --unicode=UTF-8
//require:NATIVE_MULTIBYTE_CHARS_SUPPORTED_WITH_UNICODE 1

extern "C" int printf(const char *,...);
extern "C" int strcmp(const char *, const char *);

int اختبار() {
  const char *p;
  const char *x = __FUNCTION__;
  const char *y = "اختبار";
  printf("Strings should not be equal: __FUNCTION__ should preserve the\n"
         "UTF-8 encoding, while the literal should be \"??????\".\n");
  for (p = x; *p != 0; ++p) {
    printf("%02x ", (unsigned char)*p);
  }
  printf("\n");
  for (p = y; *p != 0; ++p) {
    printf("%02x ", (unsigned char)*p);
  }
  printf("\n");
  return strcmp(x, y) == 0;
}

int main() {
  return اختبار();
}

