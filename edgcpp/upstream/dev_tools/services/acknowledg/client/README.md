# AcknowlEDG Client

AcknowlEDG is a Vue.js (version 3) app for reviewing EDG test and benchmark
results.

## Project Setup

```sh
npm install
```

### Compile and Hot-Reload for Development

```sh
npm run dev
```

### Compile and Minify for Production

Please be sure to set the environment variable
`VITE_EDG_ACKNOWLEDG_API_HOST` to the hostname of the API server including
the URL scheme (and if necessary port), e.g.:

```sh
export VITE_EDG_ACKNOWLEDG_API_HOST=wss://your-acknowledg-api.example.com
# OR (if needing a port)
export VITE_EDG_ACKNOWLEDG_API_HOST=wss://your-acknowledg-api.example.com:9422
```

Once this environment variable is set (tip: you can also create an `.env` file
for `vite` to read from), you can proceed.

There's a script (`build.sh`) that automates installing packages, running the
build, and creating a file named "dist.tar.gz" with the results.

To simply generate the `dist/` directory in an environment where the packages
have already been installed (and/or `dist.tar.gz` is undesirable) one can use
the following command:

```sh
npm run build
```
