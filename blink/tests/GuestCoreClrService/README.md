# Supplemental service diagnosis

This ordinary IL Kestrel application retains the health, echo and HTTP stop
routes, adding phase markers and an exception type/HRESULT/message report via
ordinary libc write. It avoids Exception.ToString stack-trace formatting.
The original CoreClrRuntime service remains the qualification target.
Diagnostic runs always have fresh_qualification=false; stderr markers deliberately
do not satisfy the original strict public service oracle.
