extern int startup_registration_count;
__attribute__((constructor)) inline void register_startup_occurrence() {
    ++startup_registration_count;
}
