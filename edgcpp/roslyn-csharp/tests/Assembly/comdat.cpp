template<class T> inline T assembly_identity(T value) { return value; }
template int assembly_identity<int>(int);
int call_identity(void) { return assembly_identity(3); }
