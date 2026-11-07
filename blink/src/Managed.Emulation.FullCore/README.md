# Selected FullCore producer input

The default threaded build consumes 25 byte-exact generated native14 compilation
units from `Producer/native14-processed.zip`. The archive is a **required local
input**, deliberately excluded from Git under the checkpoint's no-generated-bulk
policy. A fresh checkout must restore it before building:

```sh
python3 -B blink/scripts/restore-full-core-input.py /absolute/path/native14-processed.zip
```

The installer verifies the 4,409,099-byte input against
`Producer/manifest.json` (SHA256
`e75000b2f921bffb8afaf83ec8730d2e60db7464595581ac7439f10f1181acef`).
MSBuild verifies the archive and each ordered compilation unit on every build.
The checkpoint's retained copy is at
`/mnt/storage/blink-full-jit-checkpoint-inputs-20261107/native14-processed.zip`;
the existing workspace copy is already installed. Neither build path depends on
the deleted performance/build scratch tree. This input is not downloadable from
an asserted public release URL, and the Git checkpoint alone is not a complete
producer distribution.

The archive includes legal notices, source identities, original producer receipt
and the compiler-resource proof. Only two embedded authored runtime-source
resources changed to implement the bounded mutex lookup cache; compiler IL was
unchanged. Absolute source paths affect private generated identifiers, including
the RNG binding. A new translation is a new producer requiring explicit binding
and qualification; it must not be substituted as byte-equivalent.

The existing relative host links, defines, assembly identity, ABI7 and producer
protocol are retained. Windows execution and asynchronous unwind are unverified.
Full stays opt-in and performance acceptance remains unmet. See
[the checkpoint](../../artifacts/jit-implementation/full-jit-20261027-r1/CHECKPOINT.md).
