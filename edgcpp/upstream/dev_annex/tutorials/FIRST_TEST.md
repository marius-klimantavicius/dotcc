# Writing Your First Test

> [!NOTE]
>
> This tutorial assumes a Docker development environment.

To write your first test create a new file in the `tests/tests/sandbox`
sub-directory of the main project directory called
`simple-friend-test.sft.cpp` with the following contents:

```
//remark:Simple friend functions in class template
//options::;cn
template <class T> class A {
  friend int f(A);
  int     i;
};

A<int>  a;

int f(A<int> a) {
  return a.i;     // Should be OK
}

#if TEST_NUMBER==2
int f(A<int> a, int i) {
  return a.i;     // No access -- should fail
}
#endif
```

and run `edg-docker-test --posture quick-c-fe sandbox` you should see output
like the following:

```
+ sandbox/simple-friend-test.sft.cpp cn::PASS--NO PREVIOUS OUTPUT:Simple friend functions in class template
+ sandbox/simple-friend-test.sft.cpp cp::PASS--NO PREVIOUS OUTPUT:Simple friend functions in class template
  Created or updated after last expectations update
1/1 (100%) in 00:00:00 ~9/sec (00:00:00 remaining)
Wrote log: $TEST_KIT_DIR/tests/runs/2026.06.29-14.50.23/edg_x86_64/executed.elist
Wrote log: $TEST_KIT_DIR/tests/runs/2026.06.29-14.50.23/edg_x86_64/changes.elog
```

You can then review this output with `edg-test-run-diff`.  As this output is
the desired output for this test, the initial recording can be taken with:

```
edg-docker-test -W sandbox/simple-friend-test
```

> [!TIP]
>
> The `-W` recording flag is not limited for making an initial recording,
> it can also be used for updating recordings that have "improved".

Once the recording is updated, rerunning
`edg-docker-test --posture quick-c-fe sandbox/simple-friend-test` will now
print only:

```
1/1 (100%) in 00:00:00 ~4/sec (00:00:00 remaining)
Wrote log: $TEST_KIT_DIR/tests/runs/2026.06.29-14.57.17/edg_x86_64/executed.elist
```

If you then make a change to intentionally break the front end (for instance
adding `unexpected_condition();` to the start of `fe_translation_unit_init`),
you can rerun
`edg-docker-test --posture quick-c-fe sandbox/simple-friend-test` and the test
will now be reported as failing.  You can further review the changes to
the test by again looking at the output differences with the
`edg-test-run-diff` command.

## More Information

Additional documentation on supported directives and how to specify options can
be found in the "Testing Framework" chapter of the main project documentation.
