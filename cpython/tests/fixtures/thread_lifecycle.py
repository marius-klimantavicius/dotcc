"""Real Python thread, TLS, condition, failure and lifecycle assertions."""


def workers():
    import contextvars
    import threading

    local = threading.local()
    variable = contextvars.ContextVar("worker", default="unset")
    condition = threading.Condition()
    barrier = threading.Barrier(3, timeout=5)
    results = {}
    failures = []
    proceed = False
    def worker(index):
        try:
            local.number = index
            variable.set(index * 10)
            try:
                raise ValueError(str(index))
            except ValueError as error:
                message = str(error)
            barrier.wait()
            with condition:
                assert condition.wait_for(lambda: proceed, timeout=5), "condition timed out"
            assert local.number == index and variable.get() == index * 10
            assert message == str(index)
            results[index] = sum(range(1000))
        except BaseException as error:
            failures.append(repr(error))
    threads = [threading.Thread(target=worker, args=(index,)) for index in (1, 2)]
    for thread in threads:
        thread.start()
    try:
        barrier.wait()
        assert variable.get() == "unset" and not hasattr(local, "number")
    finally:
        with condition:
            proceed = True
            condition.notify_all()
        for thread in threads:
            thread.join(5)
    assert not any(thread.is_alive() for thread in threads), "worker did not join"
    assert not failures, failures
    assert results == {1: 499500, 2: 499500}, results


def callback_workers():
    import host
    import threading

    results = []
    failures = []
    def worker(value):
        try:
            results.append(host.double(value))
        except BaseException as error:
            failures.append(repr(error))
    threads = [threading.Thread(target=worker, args=(value,)) for value in (3, 5)]
    for thread in threads:
        thread.start()
    for thread in threads:
        thread.join(5)
    assert not any(thread.is_alive() for thread in threads), "callback/GIL join deadlock"
    assert not failures, failures
    assert sorted(results) == [6, 10], results


def ordinary_errors(work):
    import os
    import sys

    def raises(expected, action):
        try:
            action()
        except expected:
            return
        raise AssertionError(f"Expected {expected.__name__}")
    raises(SyntaxError, lambda: compile("def broken(:", "bad_source.py", "exec"))
    raises(ModuleNotFoundError, lambda: __import__("_dotcc_campaign_missing_module_"))
    raises(FileNotFoundError, lambda: open(os.path.join(work, "missing.bin"), "rb"))
    raises(ZeroDivisionError, lambda: 1 / 0)
    original = sys.getrecursionlimit()
    def recur():
        return recur()
    try:
        sys.setrecursionlimit(150)
        raises(RecursionError, recur)
    finally:
        sys.setrecursionlimit(original)
    data = bytearray(b"abc")
    view = memoryview(data)
    raises(BufferError, lambda: data.extend(b"d"))
    view.release()
    data.extend(b"d")
    assert data == b"abcd"
    assert eval("40+2") == 42, "interpreter not reusable after ordinary errors"


def callback_failure():
    import host

    try:
        host.fail(7)
    except RuntimeError as error:
        assert "managed failure" in str(error), str(error)
    else:
        raise AssertionError("Managed exception did not become Python RuntimeError")
    assert host.double(21) == 42, "callback error state leaked"


CASES = {
    "threads.workers_tls_condition": workers,
    "threads.managed_callbacks": callback_workers,
    "lifecycle.ordinary_errors": ordinary_errors,
    "lifecycle.callback_failure": callback_failure,
}
