/* This TU comes first: its static function must not steal a public C API name. */
static int shared_api(int value) { return value + 100; }
static int local_helper(int value) { return value + 3; }
static int local_helper__body(int value) { return value + 5; }
static int event(int value) { return value + 7; }
int call_internal(int value) {
  return shared_api(value) + local_helper(value) + local_helper__body(value) + event(value);
}
