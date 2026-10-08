extern void host_startup_failure(void);
int startup_attempts;
int read_startup_attempts(void) { return startup_attempts; }
__attribute__((constructor)) static void fail_once(void) {
    ++startup_attempts;
    host_startup_failure();
}
int main(void) { return 99; }
