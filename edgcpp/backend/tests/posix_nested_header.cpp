// EDG itself includes dirent.h inside namespace edg. The functions have C
// linkage, while DIR/dirent have namespaced type identities and portable layout.
#include <sys/types.h>
namespace Files {
#include <dirent.h>
}
int main() {
  Files::DIR *directory = Files::opendir(".");
  if (!directory) return 1;
  bool found = false;
  while (Files::dirent *entry = Files::readdir(directory)) {
    const char *expected = "nested-header-entry";
    int i = 0;
    while (entry->d_name[i] && entry->d_name[i] == expected[i]) ++i;
    if (!entry->d_name[i] && !expected[i]) found = true;
  }
  return Files::closedir(directory) == 0 && found ? 0 : 2;
}
