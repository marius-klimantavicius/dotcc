const sharedWords = new Int32Array(shared);
for (let i = 0; i < 2000; i++) Atomics.add(sharedWords, 2, 1);
const waitingView = waitBigInt ? new BigInt64Array(shared) : sharedWords;
Atomics.wait(waitingView, 0, waitBigInt ? 0n : 0, 5000);
