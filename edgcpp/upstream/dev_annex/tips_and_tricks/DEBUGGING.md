# Debugging Tips & Tricks

## Diagnostics

The majority of diagnostics can be caught when they are emitted by setting a
break point in `wrap_up_diagnostic`.

To catch warnings, break at:

```
wrap_up_diagnostic if dp->severity == es_warning
```

To catch errors, break at:

```
wrap_up_diagnostic if dp->severity == es_error or dp->severity == es_discretionary_error
```

To catch command line errors, break at:

```
wrap_up_diagnostic if dp->severity == es_command_line_error
```

To catch a specific error code, break at:

```
wrap_up_diagnostic if dp->error_code == ec_partial_spec_is_primary_template
```

## IL Writing

### Skip IL Write/Read

The default front end builds will dump the IL to a file at the end of
compilation and then load that IL file into the back end.  Occassionally, a
problem is a byproduct of this process; skipping the IL write & read step can
be a quick way to diagnose this type of issue.

Additionally, when tracking a problem that crosses this boundary, it can be
helpful to inhibit the serialization process to both improve the speed of the
debugger and to prevent unnecessary changes in the address space.

To skip the IL write & read process use the command line option:
`--set_flag=skip_il_read`.
