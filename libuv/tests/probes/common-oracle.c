#include "uv.h"
#include "strscpy.h"
#include "strtok.h"
#include <stdio.h>
int main(void) {
  char destination[4];
  char input[] = "one::three";
  char* iterator = NULL;
  printf("%u %s\n", uv_version(), uv_version_string());
  long result = uv__strscpy(destination, "hello", sizeof destination);
  printf("%ld %s\n", result, destination);
  printf("%s\n", uv__strtok(input, ":", &iterator));
  printf("%s\n", uv__strtok(NULL, ":", &iterator));
  printf("%s\n", uv__strtok(NULL, ":", &iterator));
  printf("%d\n", uv__strtok(NULL, ":", &iterator) == NULL);
  return 0;
}
