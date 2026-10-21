// Nondependent name errors are diagnosed even for uninstantiated templates.
template<class T> T unused(T value) { return missing + value; }
