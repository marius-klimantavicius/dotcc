"""Required semantic assertions; run inside the selected interpreter, not a host substitute."""


def language_objects():
    import gc
    import weakref

    def counter(start):
        value = start
        def step():
            nonlocal value
            value += 1
            return value
        return step
    count = counter(40)
    assert (count(), count()) == (41, 42)

    class Descriptor:
        def __get__(self, instance, owner):
            return 9 if instance is not None else self
    class Node:
        number = Descriptor()
        def __init__(self):
            self.cycle = self
    node = Node()
    assert node.number == 9
    dead = weakref.ref(node)
    del node
    gc.collect()
    assert dead() is None, "cyclic GC/weakref failed"
    assert (2 ** 200 + 1) - 2 ** 200 == 1
    assert [i * i for i in range(5)] == [0, 1, 4, 9, 16]
    assert {key: key + 1 for key in range(3)} == {0: 1, 1: 2, 2: 3}
    assert sorted({3, 1, 3, 2}) == [1, 2, 3]
    assert b"a\x00\xff"[1:] == b"\x00\xff"
    text = "a\x00\u03bb\U0001f40d"
    assert len(text) == 4 and text.encode("utf-8").decode("utf-8") == text
    lone = "\ud800x\udfff"
    assert lone.encode("utf-8", "surrogatepass").decode("utf-8", "surrogatepass") == lone
    try:
        lone.encode("utf-8")
    except UnicodeEncodeError:
        pass
    else:
        raise AssertionError("strict UTF-8 accepted lone surrogates")


def named_unicode_grammar():
    # Parser/Unicode contract, not an optional database convenience. eval keeps
    # missing unicodedata a case-level failure so other cases remain observable.
    import unicodedata

    assert eval(r"'\N{SNAKE}'") == "🐍"
    assert eval(r"'\N{GREEK SMALL LETTER LAMDA}'") == "λ"
    assert unicodedata.lookup("SNAKE") == "🐍"
    assert int("\u2003-3\u2002") == -3
    assert float("\u20033.14\u2002") == 3.14


def generators_coroutines_exceptions():
    import traceback

    def numbers():
        try:
            yield 1
            received = yield 2
            return received
        finally:
            cleanup.append("closed")
    cleanup = []
    iterator = numbers()
    assert next(iterator) == 1 and next(iterator) == 2
    try:
        iterator.send(7)
    except StopIteration as error:
        assert error.value == 7
    else:
        raise AssertionError("generator did not stop")
    assert cleanup == ["closed"]

    class Awaitable:
        def __await__(self):
            yield "paused"
            return 11
    async def task():
        return await Awaitable()
    coroutine = task()
    assert coroutine.send(None) == "paused"
    try:
        coroutine.send(None)
    except StopIteration as error:
        assert error.value == 11
    else:
        raise AssertionError("coroutine did not return")
    try:
        try:
            raise KeyError("cause")
        except KeyError as cause:
            raise ValueError("outer") from cause
    except ValueError as error:
        assert isinstance(error.__cause__, KeyError)
        rendered = "".join(traceback.format_exception(error))
        assert "generators_coroutines_exceptions" in rendered
        assert "KeyError: 'cause'" in rendered and "ValueError: outer" in rendered

    def add(a, b):
        return a + b
    for _ in range(1000):
        assert add(20, 22) == 42
    assert add("ab", "cd") == "abcd", "specialization/deoptimization"


def required_stdlib():
    import collections
    import contextvars
    import functools
    import io
    import itertools
    import json
    import math
    import random
    import re
    import struct
    import time

    assert json.dumps({"values": [4, 6, 10], "sum": 20}, sort_keys=True, separators=(",", ":")) == '{"sum":20,"values":[4,6,10]}'
    assert re.findall(r"\b\w+\b", "alpha λ 42") == ["alpha", "λ", "42"]
    assert list(collections.deque([1, 2, 3])) == [1, 2, 3]
    assert collections.Counter("ababa") == {"a": 3, "b": 2}
    assert functools.reduce(lambda a, b: a * b, [2, 3, 7]) == 42
    assert list(itertools.islice(itertools.count(2, 3), 4)) == [2, 5, 8, 11]
    assert math.sqrt(81) == 9 and math.copysign(1, -0.0) == -1
    assert math.isnan(float("nan")) and math.isinf(float("inf"))
    assert struct.pack("<Ih", 0x12345678, -7) == b"\x78\x56\x34\x12\xf9\xff"
    assert struct.unpack("<Ih", b"\x78\x56\x34\x12\xf9\xff") == (0x12345678, -7)
    assert random.Random(123).getrandbits(32) == 224899942
    stream = io.StringIO("λ🐍")
    assert stream.read(1) == "λ" and stream.read() == "🐍"
    variable = contextvars.ContextVar("campaign", default=0)
    token = variable.set(42)
    assert contextvars.copy_context().run(variable.get) == 42
    variable.reset(token)
    assert variable.get() == 0
    before = time.monotonic()
    time.sleep(0.01)
    assert 0.005 <= time.monotonic() - before < 5, "bounded monotonic sleep"


def files(work):
    import os
    import pathlib

    root = pathlib.Path(work) / "λ🐍"
    root.mkdir()
    path = root / "data.bin"
    data = b"a\x00\xff"
    path.write_bytes(data)
    assert path.read_bytes() == data
    moved = root / "moved.bin"
    os.replace(path, moved)
    assert not path.exists() and moved.stat().st_size == 3
    assert sorted(os.listdir(root)) == ["moved.bin"]
    moved.unlink()
    root.rmdir()


CASES = {
    "language.objects_gc_unicode": language_objects,
    "language.named_unicode_grammar": named_unicode_grammar,
    "language.generators_coroutines_exceptions": generators_coroutines_exceptions,
    "stdlib.required": required_stdlib,
    "stdlib.files": files,
}
