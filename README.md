# About
Visual Studio has built-in support for [Docker Desktop](https://www.docker.com/products/docker-desktop/), but it doesn't support [Podman Desktop](https://podman-desktop.io/), at least not yet. Attempting to add Docker support to a project will cause VS to complain that Docker Desktop is not installed, Docker isn't running properly, etcetera. In fact, Podman Desktop can be used to replace it, with a little glue to smooth over the differences. This project is the glue needed to make `docker-compose` work.

# Getting started
To start developing containers with VS and Podman, you need to do the following:
- First, ensure Docker Desktop is not installed, and if it is, uninstall it. This is optional, and while Podman Desktop supports Docker as a container engine, if you're not intending to use Docker in the first place things are much easier to troubleshoot if you know that it's not there to give you trouble.
- Install [Podman](https://github.com/containers/podman/blob/main/docs/tutorials/podman-for-windows.md), [Podman Desktop](https://podman-desktop.io/) and [podman-compose](https://github.com/containers/podman-compose#installation), in that order. Make sure to follow the instructions and test that you can at least spin up a container (and ideally also build a compose file) before trying to make things work with VS.
- Copy `podman.exe` to `docker.exe`, somewhere in your `PATH` (you can of course just use the same directory as where you find `podman.exe`, if you have write permission). If you're not sure where `podman.exe` lives, open a command prompt and use `where podman.exe` to find it.
- Build this project, publish it (this will produce a single file), and copy the resulting `docker-compose.exe` to somewhere in your `PATH`.

Note that Visual Studio explicitly invokes `docker.exe` and `docker-compose.exe`. Attempting to provide `.cmd` or `.ps1` wrappers will not work.

# Verifying
To verify that it works, create a simple "Hello, world" console project, right-click on the project and choose `Add -> Docker support`. This should enable building and debugging your app in a container. Then right-click and choose `Add -> Container orchestrator support` to create a `docker-compose` project. If this builds, chances are pretty good debugging will work as well.

If you run into trouble, switch to the Output window (`Debug -> Windows -> Output`) and select "Build" for "Show output from". You should see lines like the following:
>`docker-compose  -f "C:\AwesomeProject\docker-compose.yml" -f "C:\AwesomeProject\docker-compose.override.yml" -f "C:\AwesomeProject\obj\Docker\docker-compose.vs.release.partial.g.yml" -p dockercompose12133038801188878165 --no-ansi kill`

>`podman-compose -f C:\AwesomeProject\docker-compose.yml -f C:\AwesomeProject\docker-compose.override.yml -f C:\AwesomeProject\obj\Docker\docker-compose.vs.release.partial.g.yml.munged -p dockercompose12133038801188878165 --no-ansi kill --all`

The first line is what VS is executing, the second line is output from the wrapper to tell you what *it* is executing after translation. Not all error messages are problematic; VS will do things like unconditionally stop containers even if they're not actually running.

# Limitations
The wrapper is fairly crude and I basically just banged away at it until it worked in the most basic sense. Not all command VS issues may be properly translated, so you may find that you need to manually delete containers to clean things up, as (for example) `podman compose down` doesn't support the `--rmi local` option yet. It's also easy for this to break as newer versions of VS or Podman are released, but such is life.

The "Containers" window (`View -> Other windows -> Containers`) seems to work only outside debugging, and not at all with composed containers. VS will complain that "the current Docker context is not supported", and the "Switch to the default context" link does nothing. I haven't investigated where this is coming from and if it can be fixed, because the functionality on offer is mostly duplicated by Podman Desktop anyway. Use that instead.

# Further work
Since this wrapper does nothing very complicated, it could be easily ported to other languages (like, say, Python); making it potentially easier to build and distribute. Anyone interested in that should feel free to reverse engineer the logic and reimplement it.