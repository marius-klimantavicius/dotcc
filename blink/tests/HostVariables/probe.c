#define _GNU_SOURCE 1
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#ifdef BLINK_MANAGED_VARIABLES
#include "host-variables.h"
#endif
#define CHECK(x) do { if(!(x)){printf("variable failure %d\n",__LINE__);return __LINE__;} } while(0)
int VariablesProbe(void) {
  char *value,*empty,*utf8;
  errno=123;value=getenv("BLINK_TEST_VALUE");
  CHECK(value && !strcmp(value,"private-value") && errno==123);
  empty=getenv("BLINK_TEST_EMPTY");CHECK(empty && !*empty && errno==123);
  utf8=getenv("BLINK_TEST_UTF8");CHECK(utf8 && !strcmp(utf8,"\xc3\xa9/\xce\xbb") && errno==123);
  CHECK(!getenv("BLINK_TEST_MISSING") && errno==123);
  CHECK(!getenv("") && errno==123);
  CHECK(!getenv("X=Y") && errno==123);
  CHECK(getenv("BLINK_TEST_VALUE")==value && !strcmp(value,"private-value"));
  CHECK(getenv("BLINK_TEST_EMPTY")==empty && getenv("BLINK_TEST_UTF8")==utf8);
  puts("immutable private environment lookup, errno and stable value pointers: PASS");
  return 0;
}
#ifdef BLINK_MANAGED_VARIABLES
void *VariablesSave(void) { return getenv("BLINK_TEST_VALUE"); }
int VariablesSaved(void *value, int second) {
  CHECK(value==getenv("BLINK_TEST_VALUE"));
  CHECK(!strcmp(value,second ? "other-value" : "private-value"));
  return 0;
}
int VariablesPrivate(void) {
  char bad[2]={(char)0xc0,0},longname[1026];
  CHECK(!getenv(0) && errno==EFAULT);
  CHECK(!getenv(bad) && errno==EINVAL);
  memset(longname,'x',1025);longname[1025]=0;
  CHECK(!getenv(longname) && errno==36);
  errno=123;CHECK(!getenv("BLINK_FORBIDDEN_OS_VALUE") && errno==123);
  return 0;
}
int VariablesUnavailable(int error) { CHECK(!getenv("BLINK_TEST_VALUE") && errno==error);return 0; }
int VariablesDefaultEmpty(void) {
  errno=123;CHECK(!getenv("BLINK_TEST_VALUE") && !getenv("BLINK_FORBIDDEN_OS_VALUE") && errno==123);
  return 0;
}
#else
int main(void){return VariablesProbe();}
#endif
