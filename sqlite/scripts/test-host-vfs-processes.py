#!/usr/bin/env python3
"""Opt-in process oracle for the real VFS; no subprocesses in dotcc libraries.

Run against JIT or NativeAOT; --native adds pinned native SQLite interoperability.
Every server has bounded protocol waits, and crashes use an actual forced kill.
"""
import argparse
import contextlib
import os
from pathlib import Path
import queue
import subprocess
import tempfile
import threading
import uuid


def require(condition, message):
    if not condition:
        raise AssertionError(message)


class Server:
    active = []

    def __init__(self, command, path, logs, native=False, readonly=False):
        self.log = logs / ("server-" + uuid.uuid4().hex + ".log")
        self.errors = self.log.open("w", encoding="utf-8")
        args = command + ([] if native else ["serve"]) + [str(path)]
        if readonly:
            args.append("readonly")
        self.process = subprocess.Popen(args, stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                                        stderr=self.errors, text=True, encoding="utf-8", bufsize=1)
        self.lines = queue.Queue()
        self.reader = threading.Thread(target=self._pump, daemon=True)
        self.reader.start()
        Server.active.append(self)
        require(self.read() == "READY", "server readiness")

    def _pump(self):
        for line in self.process.stdout:
            self.lines.put(line.rstrip("\r\n"))
        self.lines.put(None)

    def read(self):
        try:
            value = self.lines.get(timeout=30)
        except queue.Empty:
            raise AssertionError("Timed out waiting for VFS server; " + self.log.read_text()) from None
        require(value is not None, "VFS server exited unexpectedly: " + self.log.read_text())
        return value

    def command(self, text):
        self.process.stdin.write(text + "\n")
        self.process.stdin.flush()
        return self.read()

    def execute(self, sql, expected=0):
        result = self.command("E " + sql)
        require(result == "RC " + str(expected), f"{sql}: expected RC {expected}, got {result}; {self.log.read_text()}")

    def query(self, sql, expected):
        result = self.command("Q " + sql)
        require(result == "ROW " + expected, f"{sql}: expected {expected!r}, got {result!r}")

    def close(self, crash=False):
        if self not in Server.active:
            return
        try:
            if crash:
                self.process.kill()
            else:
                self.process.stdin.write("X\n")
                self.process.stdin.flush()
            code = self.process.wait(timeout=15)
            if not crash:
                require(code == 0, "server close: " + self.log.read_text())
        finally:
            if self.process.poll() is None:
                self.process.kill()
                self.process.wait(timeout=15)
            self.reader.join(timeout=5)
            self.process.stdin.close()
            self.process.stdout.close()
            self.errors.close()
            Server.active.remove(self)

    def __enter__(self):
        return self

    def __exit__(self, exception_type, exception, traceback):
        self.close(crash=exception is not None)


def open_server(kind, path, directory, readonly=False):
    command, native = kind
    return Server(command, path, directory, native=native, readonly=readonly)


def persistence(writer, reader, directory, label):
    path = directory / ("persistence-" + label + "-λ.db")
    with open_server(writer, path, directory) as connection:
        connection.execute("PRAGMA synchronous=FULL;CREATE TABLE data(id INTEGER PRIMARY KEY,payload BLOB);"
                           "INSERT INTO data VALUES(1,jsonb('{\"name\":\"λ\",\"n\":42}'));"
                           "CREATE VIRTUAL TABLE docs USING fts5(body);INSERT INTO docs VALUES('café Ωμέγα database');")
    require(path.read_bytes()[:16] == b"SQLite format 3\0", "durable SQLite file signature")
    with open_server(reader, path, directory, readonly=True) as connection:
        connection.query("SELECT json_extract(payload,'$.name') FROM data", "λ")
        connection.query("SELECT count(*) FROM docs WHERE docs MATCH 'cafe'", "1")
        connection.query("SELECT count(*) FROM docs WHERE docs MATCH 'ωμέγα'", "1")
        connection.execute("UPDATE data SET payload=NULL", expected=8)
        connection.query("PRAGMA integrity_check", "ok")
    print("PASS real-file persistence, JSONB/FTS5 and readonly:", label, flush=True)


def lock_protocol(writer_kind, reader_kind, directory, label):
    path = directory / ("locking-" + label + ".db")
    with open_server(writer_kind, path, directory) as writer:
        writer.execute("CREATE TABLE data(id INTEGER PRIMARY KEY,value INTEGER);INSERT INTO data VALUES(1,7)")
        with open_server(reader_kind, path, directory) as reader:
            reader.execute("BEGIN")
            reader.query("SELECT value FROM data", "7")
            writer.execute("BEGIN IMMEDIATE")
            # A RESERVED writer still permits a new readonly process.
            with open_server(reader_kind, path, directory, readonly=True) as probe:
                probe.query("SELECT value FROM data", "7")
            reader.execute("UPDATE data SET value=99", expected=5)
            writer.execute("UPDATE data SET value=42")
            writer.execute("COMMIT", expected=5)  # Retains PENDING until readers leave.
            with open_server(reader_kind, path, directory) as late:
                late.execute("SELECT value FROM data", expected=5)
            reader.execute("ROLLBACK")
            writer.execute("COMMIT")
            reader.query("SELECT value FROM data", "42")
            writer.execute("BEGIN EXCLUSIVE")
            reader.execute("SELECT value FROM data", expected=5)
            writer.execute("ROLLBACK")
            reader.query("SELECT value FROM data", "42")
    print("PASS native-compatible shared/reserved/pending/exclusive locks:", label, flush=True)


def crash_recovery(writer_kind, reader_kind, directory, label):
    path = directory / ("recovery-" + label + ".db")
    writer = open_server(writer_kind, path, directory)
    writer.execute("PRAGMA synchronous=FULL;PRAGMA journal_mode=DELETE;CREATE TABLE data(id INTEGER PRIMARY KEY,value INTEGER,pad BLOB);"
                   "WITH RECURSIVE n(x) AS (VALUES(1) UNION ALL SELECT x+1 FROM n WHERE x<100) "
                   "INSERT INTO data SELECT x,7,zeroblob(4000) FROM n;")
    writer.execute("PRAGMA cache_size=3;PRAGMA cache_spill=ON;BEGIN IMMEDIATE;UPDATE data SET value=99,pad=randomblob(4000)")
    journal = Path(str(path) + "-journal")
    require(journal.exists() and journal.stat().st_size > 4096, "crash test must create a real rollback journal")
    require(journal.read_bytes()[:8] == bytes.fromhex("d9d505f920a163d7"), "crash test must have a synced hot-journal header")
    writer.close(crash=True)
    with open_server(reader_kind, path, directory) as recovery:
        recovery.query("SELECT sum(value) FROM data", "700")
        recovery.query("PRAGMA integrity_check", "ok")
        recovery.execute("BEGIN IMMEDIATE;UPDATE data SET value=8 WHERE id=1;COMMIT")
        recovery.query("SELECT sum(value) FROM data", "701")
    print("PASS forced process death, hot-journal recovery and released OS locks:", label, flush=True)


def aliases(managed, native, directory):
    target = directory / "alias-target.db"
    with open_server(managed, target, directory) as writer:
        writer.execute("CREATE TABLE data(value);INSERT INTO data VALUES(42)")
        hard = directory / "hardlink.db"
        os.link(target, hard)
        writer.execute("BEGIN EXCLUSIVE")
        for reader_kind in [managed] + ([native] if native else []):
            with open_server(reader_kind, hard, directory) as reader:
                reader.execute("SELECT value FROM data", expected=5)
        writer.execute("ROLLBACK")
        symbolic = directory / "symbolic.db"
        try:
            os.symlink(target.name, symbolic)
        except OSError:
            if os.name != "nt":
                raise
            print("SKIP symbolic link test: Windows account lacks symlink privilege", flush=True)
            return
        with open_server(managed, symbolic, directory) as reader:
            reader.query("SELECT value FROM data", "42")
            reader.execute("BEGIN IMMEDIATE;UPDATE data SET value=43")
            require(Path(str(target) + "-journal").exists(), "symlink must use canonical journal path")
            require(not Path(str(symbolic) + "-journal").exists(), "no second journal through symlink alias")
            reader.execute("ROLLBACK")
        # Symlink + '..' must resolve before path normalization.
        nested = directory / "nested"
        nested.mkdir()
        deeper = nested / "deeper"
        deeper.mkdir()
        hop = directory / "hop"
        os.symlink(deeper, hop, target_is_directory=True)
        alias_path = hop / ".." / "through-link.db"
        with open_server(managed, alias_path, directory) as reader:
            reader.execute("CREATE TABLE t(value)")
        require((nested / "through-link.db").exists(), "symlink/.. filesystem semantics")
        require(not (directory / "through-link.db").exists(), "no lexical-only symlink normalization")
    print("PASS inode alias locking and canonical symlink journal paths", flush=True)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--managed", nargs="+", required=True)
    parser.add_argument("--native", type=Path)
    args = parser.parse_args()
    managed = (args.managed, False)
    native = ([str(args.native.resolve())], True) if args.native else None
    artifacts = Path(__file__).resolve().parent.parent / "artifacts"
    artifacts.mkdir(exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="host-vfs-processes-", dir=artifacts) as temporary:
        directory = Path(temporary)
        try:
            pairs = [(managed, managed, "managed-managed")]
            if native:
                pairs += [(native, managed, "native-managed"), (managed, native, "managed-native")]
            for writer, reader, label in pairs:
                persistence(writer, reader, directory, label)
                lock_protocol(writer, reader, directory, label)
                crash_recovery(writer, reader, directory, label)
            aliases(managed, native, directory)
        finally:
            for server in list(Server.active):
                with contextlib.suppress(Exception):
                    server.close(crash=True)
    print("PASS host VFS independent-process campaign", flush=True)


if __name__ == "__main__":
    main()
