// Function references and function-pointer objects require distinct lowering.
int duckdb_function_calls;
int duckdb_function_selections;
int target(int value) { ++duckdb_function_calls; return value + 1; }
int doubled(int value) { ++duckdb_function_calls; return value * 2; }
int no_throw(int value) noexcept { ++duckdb_function_calls; return value - 1; }
using Callback = int(int);
Callback &select_function(bool first) {
    ++duckdb_function_selections;
    return first ? target : doubled;
}
template<class F> int invoke(F &&function, int value) {
    return static_cast<F &&>(function)(value);
}
struct CallbackHolder { Callback *callback; };
int main() {
    Callback *pointer = target;
    CallbackHolder holder{doubled};
    int a = invoke(target, 2);
    int b = invoke(select_function(false), 3);
    int c = invoke(pointer, 5);
    int d = invoke(holder.callback, 4);
    int e = invoke(no_throw, 8);
    return a == 3 && b == 6 && c == 6 && d == 8 && e == 7 &&
        duckdb_function_calls == 5 && duckdb_function_selections == 1 ? 0 : 1;
}
