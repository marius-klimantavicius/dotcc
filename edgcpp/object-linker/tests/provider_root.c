extern int (*provider_callback)(int);
extern int provider_api(int);
int public_api_not_called_by_main(void) { return provider_api(4); }
int main(void) { return provider_callback(7) == 22 ? 0 : 1; }
