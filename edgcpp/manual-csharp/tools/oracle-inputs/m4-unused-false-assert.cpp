// P2593R1/CWG2518: false static_assert in an uninstantiated template is deferred.
template<class T> int assertion(T value) { static_assert(false); return 1; }
