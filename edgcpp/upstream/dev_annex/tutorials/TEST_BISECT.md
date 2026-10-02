# Automatic Test Bisection

> [!NOTE]
>
> This tutorial assumes a Docker development environment.

To use automatic test bisection (i.e., looking for the commit that caused a
regression) you need three things:

- The suspected bad (new) commit
- The suspected good (old) commit
- A test case that fails on the bad commit and passes on the good commit

**OR**, if instead looking for the commit that fixed a problem:

- The suspected good (new) commit
- The suspected bad (old) commit
- A test case that fails on the bad commit and passes on the good commit

> [!WARNING]
>
> `edg-docker-test-bisect` will overwrite front end source files in your
> checkout of the repository as it runs.

With these things present, execute the command:

```
edg-docker-test-bisect <bad commit> <good commit> <path to test file>
```

> [!TIP]
>
> You can try the flag `--posture=quick-c-fe` (the linux-gcc-debug cmake build
> preset testing using the edg_x86_64 -- C-generating -- test config only) to
> accelerate the bisection process.

`<path to test file>` itself can be specified as:
- a relative path of any of the following forms:
  - `sandbox/foo.sft.cpp` (recommended for brevity),
  - `tests/sandbox/foo.sft.cpp`,
  - `tests/tests/sandbox/foo.sft.cpp`,
- a relative path from the current working directory,
- an absolute path to the test file

> [!NOTE]
>
> The test file itself must be located somewhere in a valid test suite.  If
> unsure, the `sandbox` suite is a good choice if intending to throw away the
> test.

The tooling will automatically detect which situation is being searched for,
verify there is a difference in behavior between the bad and good commits
for the given test, and then run automtically until it finds the commit
that resulted in the change in behavior.

Once this commit is found, the front end source files will be restore to the
`HEAD` commit's state (which remains untouched throughout the process).
