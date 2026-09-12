/* Valid C17; reduced from msquic_posix.h QUIC_EVENT_COMPLETION. */
typedef void (Callback)(int value);
typedef Callback *Handler;
void invoke(Handler callback) { callback(42); }
