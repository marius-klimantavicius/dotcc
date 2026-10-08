#include "project_cpp_startup_shared.h"
int startup_registration_count;
int main() { return startup_registration_count != 2; }
