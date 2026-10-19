using Managed.CPython;

if (args.Length != 2) throw new ArgumentException("Expected actual profile stdlib and diagnostic output.");
using var runtime = PythonRuntime.Initialize(new PythonOptions(args[0], AppContext.BaseDirectory));
runtime.Execute("""
import os, time, json, errno
availability = {
    'os_callables': sorted(name for name in dir(os) if callable(getattr(os, name))),
    'time_callables': sorted(name for name in dir(time) if callable(getattr(time, name))),
    'process_identity': {name: hasattr(os, name) for name in
        ('getpid', 'getppid', 'getuid', 'geteuid', 'getgid', 'getegid', 'getgroups')},
    'strftime_epoch': time.strftime('%Y-%m-%d %H:%M:%S', time.gmtime(0)),
    'wait_exitcode': os.waitstatus_to_exitcode(0x2a00),
    'child_execution': 0,
}
try:
    os.waitpid(-1, os.WNOHANG)
except OSError as error:
    availability['waitpid_errno'] = error.errno
    availability['waitpid_rejection_matches_contract'] = error.errno == errno.EPERM
else:
    raise AssertionError('waitpid unexpectedly returned success')
""", "profile_api_availability.py");
using var result = runtime.Evaluate("json.dumps(availability, sort_keys=True)");
File.WriteAllText(args[1], result.AsString() + "\n");
