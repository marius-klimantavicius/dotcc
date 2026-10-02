//type: fn
//options:  --c++20
# 1 "SemaCXX/warn-unsafe-buffer-usage-pragma-misuse.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 427 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-unsafe-buffer-usage-pragma-misuse.cpp" 2



void beginUnclosed(int * x) {
#pragma clang unsafe_buffer_usage begin

#pragma clang unsafe_buffer_usage begin
  x++;
#pragma clang unsafe_buffer_usage end
}

void endUnopened(int *x) {
#pragma clang unsafe_buffer_usage end

#pragma clang unsafe_buffer_usage begin
  x++;
#pragma clang unsafe_buffer_usage end
}

void wrongOption() {
#pragma clang unsafe_buffer_usage start
#pragma clang unsafe_buffer_usage close
}

void unclosed(int * p1) {
#pragma clang unsafe_buffer_usage begin


# 1 "SemaCXX/warn-unsafe-buffer-usage-pragma.h" 1


p1++;
# 30 "SemaCXX/warn-unsafe-buffer-usage-pragma-misuse.cpp" 2
#pragma clang unsafe_buffer_usage end


#pragma clang unsafe_buffer_usage begin
}
