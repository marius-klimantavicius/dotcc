function atomicCheck(ok, label) { if (!ok) throw Error(label); }
function atomicThrows(type, fn) {
    try { fn(); } catch (e) { atomicCheck(e instanceof type, 'wrong exception: ' + e); return; }
    throw Error('missing ' + type.name);
}
__case('atomics-integer-widths', () => {
    for (const T of [Int8Array, Uint8Array, Int16Array, Uint16Array, Int32Array, Uint32Array]) {
        const a = new T(new SharedArrayBuffer(16));
        a[1] = 23; a[2] = 29; a[3] = 31;
        atomicCheck(Atomics.store(a, 0, 12) === 12 && Atomics.load(a, 0) === 12, T.name + ' load/store');
        atomicCheck(Atomics.add(a, 0, 5) === 12 && Atomics.sub(a, 0, 2) === 17, T.name + ' add/sub old value');
        atomicCheck(Atomics.and(a, 0, 6) === 15 && Atomics.or(a, 0, 8) === 6 && Atomics.xor(a, 0, 3) === 14, T.name + ' bitwise');
        atomicCheck(Atomics.exchange(a, 0, 31) === 13 && Atomics.compareExchange(a, 0, 99, 8) === 31 && a[0] === 31, T.name + ' failed CAS');
        atomicCheck(Atomics.compareExchange(a, 0, 31, 7) === 31 && a[0] === 7, T.name + ' successful CAS');
        Atomics.store(a, 0, -1);
        const expected = new T([-1])[0];
        atomicCheck(Atomics.load(a, 0) === expected && Atomics.add(a, 0, 1) === expected && a[0] === 0, T.name + ' wrap');
        atomicCheck(a[1] === 23 && a[2] === 29 && a[3] === 31, T.name + ' adjacent elements preserved');
        atomicCheck(Atomics.store(a, 1, 7) === 7 && Atomics.load(a, 1) === 7 && a[0] === 0 && a[2] === 29, T.name + ' interior element width');
    }
});
__case('atomics-bigint-widths', () => {
    for (const T of [BigInt64Array, BigUint64Array]) {
        const a = new T(new SharedArrayBuffer(16));
        const high = (1n << 60n) + 12n;
        atomicCheck(Atomics.store(a, 0, high) === high && Atomics.load(a, 0) === high, T.name + ' exact64');
        atomicCheck(Atomics.add(a, 0, 5n) === high && Atomics.sub(a, 0, 2n) === high + 5n, T.name + ' add/sub');
        atomicCheck(Atomics.and(a, 0, 15n) === high + 3n && Atomics.or(a, 0, 32n) === 15n && Atomics.xor(a, 0, 3n) === 47n, T.name + ' bitwise');
        atomicCheck(Atomics.exchange(a, 0, 31n) === 44n && Atomics.compareExchange(a, 0, 99n, 8n) === 31n && a[0] === 31n, T.name + ' failed CAS');
        atomicCheck(Atomics.compareExchange(a, 0, 31n, 7n) === 31n && a[0] === 7n, T.name + ' successful CAS');
        Atomics.store(a, 0, (1n << 64n) - 1n);
        const expected = new T([-1n])[0];
        atomicCheck(Atomics.load(a, 0) === expected && Atomics.add(a, 0, 1n) === expected && a[0] === 0n, T.name + ' wrap64');
    }
});
__case('atomics-validation-and-coercion', () => {
    const a = new Int32Array(new SharedArrayBuffer(16));
    let indexCalls = 0, valueCalls = 0;
    atomicCheck(Atomics.store(a, {valueOf(){indexCalls++;return 1;}}, {valueOf(){valueCalls++;return 42;}}) === 42, 'store coercion');
    atomicCheck(indexCalls === 1 && valueCalls === 1 && a[1] === 42, 'coercion exactly once');
    atomicThrows(RangeError, () => Atomics.load(a, -1));
    atomicThrows(RangeError, () => Atomics.load(a, a.length));
    for (const T of [Float32Array, Float64Array, Uint8ClampedArray])
        atomicThrows(TypeError, () => Atomics.add(new T(4), 0, 1));
    atomicThrows(TypeError, () => Atomics.add(a, 0, 1n));
    atomicThrows(TypeError, () => Atomics.add(new BigInt64Array(1), 0, 1));
    atomicThrows(TypeError, () => Atomics.wait(new Int32Array(1), 0, 0, 0));
    atomicThrows(TypeError, () => Atomics.wait(new Uint32Array(new SharedArrayBuffer(4)), 0, 0, 0));
    atomicCheck(Atomics.notify(a, 0, 0) === 0 && Atomics.notify(a, 0) === 0, 'no waiters');
});
__case('atomics-lockfree-and-pause', () => {
    for (const size of [1, 2, 4, 8]) atomicCheck(Atomics.isLockFree(size) === true, 'lockfree ' + size);
    for (const size of [0, 3, 16]) atomicCheck(Atomics.isLockFree(size) === false, 'nonwidth ' + size);
    atomicCheck(Atomics.pause() === undefined && Atomics.pause(1) === undefined, 'pause');
    atomicThrows(TypeError, () => Atomics.pause(1.5));
});
__case('atomics-default-nonblocking', () => {
    atomicThrows(TypeError, () => Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, 0));
});
