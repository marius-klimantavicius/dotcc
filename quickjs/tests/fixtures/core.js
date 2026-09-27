function check(ok, label) { if (!ok) throw Error(label); }
__case('closure', () => { const f = x => () => ++x; const g = f(4); check(g() === 5 && g() === 6, 'closure'); });
__case('unicode-nul-surrogate', () => { const s = 'A\0\ud83d\ude00\ud800'; check(s.length === 5 && s.charCodeAt(4) === 0xd800 && JSON.parse(JSON.stringify(s)) === s, 'utf16'); });
__case('bigint-boundaries', () => { check(((1n << 127n) - 1n).toString() === '170141183460469231731687303715884105727', 'bigint'); check(BigInt('9223372036854775808') * -1n === -9223372036854775808n, 'signed'); });
__case('numbers', () => { check(Object.is(-0, -1 / Infinity) && Number.isNaN(0 / 0), 'numbers'); check(Number('1.0000000000000002') === 1 + Number.EPSILON, 'round trip'); for (const x of [1e-20,-1e-20]) { check(Math.expm1(x) === x, 'tiny expm1'); check(Math.log1p(x) === x, 'tiny log1p'); } check(Object.is(Math.expm1(-0), -0) && Object.is(Math.log1p(-0), -0), 'negative zero math'); check(Math.abs(Math.hypot(3e200,4e200) / 5e200 - 1) <= 2 * Number.EPSILON, 'scaled hypot'); });
__case('regexp', () => { check(/^\p{Letter}+$/u.test('Žąsis') && /(a+)b\1/.exec('aabaa')[1] === 'aa', 'regexp'); });
__case('map-set', () => { const key = {}; check(new Map([[key, 42]]).get(key) === 42 && new Set([1,1,2]).size === 2, 'collections'); });
__case('typed-array', () => { const b = new ArrayBuffer(8); new DataView(b).setBigInt64(0, -9223372036854775808n, true); check(new BigInt64Array(b)[0] === -9223372036854775808n, 'buffer'); });
__case('exceptions', () => { let ok = false; try { null.f(); } catch(e) { ok = e instanceof TypeError; } check(ok, 'type error'); });
__case('fixed-date', () => { check(new Date(0).toISOString() === '1970-01-01T00:00:00.000Z', 'date'); });
globalThis.order = [];
Promise.resolve().then(() => order.push(1)).then(() => { order.push(2); check(order.join() === '1,2', 'jobs'); print('CASE PASS promise-order'); });
