/* A native-looking name alone must not authorize an unsafe shared-runtime cast. */
struct stat { int st_dev; long st_size; };
extern int stat(const char *, struct stat *);
int main(void) { struct stat value; return stat(".", &value); }
