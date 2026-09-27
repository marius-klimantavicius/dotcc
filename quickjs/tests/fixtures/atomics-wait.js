__case('atomics-bounded-wait', () => {
    for (const T of [Int32Array, BigInt64Array]) {
        const a = new T(new SharedArrayBuffer(16));
        const zero = T === Int32Array ? 0 : 0n;
        const one = T === Int32Array ? 1 : 1n;
        if (Atomics.wait(a, 0, one, 100) !== 'not-equal') throw Error(T.name + ' mismatch');
        if (Atomics.wait(a, 0, zero, 0) !== 'timed-out') throw Error(T.name + ' zero timeout');
        if (Atomics.wait(a, 0, zero, 2) !== 'timed-out') throw Error(T.name + ' bounded timeout');
        if (Atomics.notify(a, 0) !== 0) throw Error(T.name + ' stale waiter after timeout');
        Atomics.store(a, 0, one);
        if (Atomics.load(a, 0) !== one) throw Error(T.name + ' recovery');
    }
});
