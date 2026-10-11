using Difference = decltype((char *)0 - (char *)0);
Difference char_distance(const char *left, const char *right);
Difference int_distance(const int *left, const int *right);
int *selected_left(int *value, int *calls);
int *selected_right(int *value, int *calls);
Difference *selected_destination(Difference *value, int *left_calls, int *right_calls, int *state);
