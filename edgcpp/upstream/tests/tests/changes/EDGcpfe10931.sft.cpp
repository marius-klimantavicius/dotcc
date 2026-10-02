//type:fn
//remark:[4.2] Missing error on invalid default exception handler
// 8/18/10  [EDGcpfe/10931]
//
// Missing error on invalid default exception handler
//
// The C++ standard requires a default exception handler to be the last handler
// associated with a try-block: The front end therefore normally issues an error
// for any handler that follows a default handler.  The front end also warns on
// cases where a handler is masked by a previous handler (e.g., because both
// handlers are for the same type).  However, if a handler X is preceded both by
// a default handler and an earlier handler that masks X, a warning was issued
// but not an error (even though such a situation is invalid).
void f() try {
} catch (int) {
} catch (...) {
} catch (int) {  // Previously accepted with a warning.  Now an error
}                // is also issued.
