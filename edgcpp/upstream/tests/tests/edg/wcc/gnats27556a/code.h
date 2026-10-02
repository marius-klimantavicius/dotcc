template<typename T>
__attribute__((__visibility__("hidden"))) void foo() { }

extern template __attribute__((__visibility__("default"))) void foo<int>();
