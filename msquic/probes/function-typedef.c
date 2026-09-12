/* Valid C17: parentheses are not the only issue in callback typedefs. */
typedef void Callback(int value);
typedef Callback *Handler;
void invoke(Handler callback) { callback(42); }
