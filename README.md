# pipe

[![NuGet Status](https://buildstats.info/nuget/pipe-cli)](https://buildstats.info/nuget/pipe-cli)

**TL;DR;** dotnet cli tool - inspired by make - for declaring and executing a pipeline.

## Introduction

`pipe` is a `dotnet` CLI tool that is available through `NuGet`. It enables you to define a pipeline for any given .NET project in a structured way that is heavily inspired by `make` and `makefile`'s. In fact, much like `make` has it's `makefile`, `pipe` has a `pipeline` file where you define the `steps` that becomes your pipeline. The `pipeline` file is just simple text file that you can edit in your favourite editor.

A `step` can be a list of `acions` that will be execute during execution of that particular step. An `action` is a command that you would execute on the command line (e.g. `dotnet build`). A `step` can also have _pre-steps_ which tells `pipe` that before executing that particular step, other steps needs to be executed first.

Last but not least, you can also define variables in the `pipeline` file, that will later be merged into the `action`s that are executed if they reference them. You can also reference environment variables within an action. Variables defined inside a `pipeline` file can be overridden by passing a new value as argument to `pipe` on the command line.

## Requirements

`pipe` - as a dotnet cli tool - currently requires .NET 5 or newer to run.

## Installation

It is highly recommended that the tool is installed per repository as a local dotnet tool. This allows you to pin the version of `pipe` so developers don't have different versions installed on their machine.

To do this it is recommended that you run the following command at the root of your .NET repository;

```shell
dotnet new tool-manifest
```

This will create a `/.config` folder in your repository with a `dotnet-tools.json` file that specifies what tools are used in this repository and it pins their version. This is very similar to other package systems e.g. `NuGet` (through the project file) and NPM's `package.json` file. The `dotnet-tools.json` file should be committed to source control.

Now you are all set to install `pipe`. This tool is available through `NuGet` and can be installed as a `dotnet` cli tool by running the following command in your terminal:

```shell
dotnet tool install pipe-cli
```

All set! You will now have access to the tool by running `dotnet pipe` on your command line.

## Usage

Let's take a closer look at how you would define a `pipeline` file and how you would use the `dotnet pipe` command.

```
dotnet pipe [step...] [VARIABLE=<value>...] [-v] [-f <path>]
```

### Example 1: define simple steps

Let's define steps for:

- `restore` --> restoring `NuGet` packages
- `build` --> compiling the source code
- `test` --> run all available unit tests in the repository

Create a simple extension less text file named `pipeline` (or `Pipeline`) and place it in the root of your repository. Define the three steps from above like this:

File: [root of your project]/pipeline

```
restore:
    dotnet restore

build:
    dotnet build

test:
    dotnet test
```

As you can see in the example above, we have defined three steps: `restore`, `build` and `test`. Each step has a single `action` defined and this is _just_ a normal command that can be executed on your command line. The action is always indented (aim for a tab or two spaces). This means that the `restore` step - when executed - will in turn execute a `dotnet restore` command on the command line.

To execute a single step defined in a `pipeline` file like the one above, you would run the following command on your command line:

```shell
dotnet pipe restore
```

This will execute the `restore` step defined above. You can also execute all three steps sequentially (e.g. `restore`, `build` and `test`) by executing the following command on your command line:

```shell
dotnet pipe restore build test
```

This will instruct `pipe` to first execute the `restore` step and if that succeeds it will continue with the `build` step and so on. If - however - a step fails, the pipeline will stop any further execution of actions and steps.

Steps can have multiple `action`s and they are executed in the order they are declared:

```
step-name:
    action1
    action2
    action3
```

`pipe` also supports multi-line `action`s where you can break up a long action by using the `\` character. It must be located at the end of a line without any trailing spaces.

```
greeting:
    echo hello \
        world
```

### Example 2: defining pre-steps

A single step can reference other steps. This will instruct `pipe` to execute any referenced steps before executing that particular step. Let's see that in action.

Given the `pipeline` file from before, we can define a single step named `all` that in turn will execute all the other steps by referencing them as a pre-step:

```
all: restore build test

restore:
    dotnet restore

build:
    dotnet build

test:
    dotnet test
```

Pre-steps a located on the same line as the step that references them:

```
step: pre-step1 pre-step2 pre-step3 etc
```

Pre-steps are also executed sequentially in the order that they are referenced.
So from the `pipeline` file above with the newly added `all` step, instead of running the whole pipeline by `dotnet pipe restore build test` you can get away with just executing:

```shell
dotnet pipe all
```

Now heres a little feature of `pipe`: the first step defined in your `pipeline` file is the default step to execute if no steps was given as argument to the `pipe` command:

```shell
dotnet pipe
```

This will default to executing the `all` step from above, as it appears first in the `pipeline` file. This is the same behaviour that you would find in `make` and `makefile`s - which is awesome!

### Example 3: define variables

The modest convention is to define variables at the top of your `pipeline` file and then reference them in your actions.

A normal thing to do with .NET projects is to define which configuration that you want the `dotnet build` command to be executed with. Should it be **debug** or **release** and can we use **debug** for local development and **release** when the pipeline is executed on the build server. Let's add a configuration variable and use it in a build step:

```
CONFIGURATION=debug

build:
    dotnet build -c $(CONFIGURATION)

```

By convention all-caps are used for variable names to make the pop out (not required though). When the above build step is executed, it's single action will have it's variable reference replaced with the value of the variable declared at the top of the pipeline file.

So variables are declared in a key/value structure like `key=value` and they are referenced by using the `$` character and parentheses like `$(key)`, in your step actions.

You can also reference one variable inside another variable with the same `$(key)` notation:

```
FOO=foo
FOOBAR=$(foo)-bar
```

So in the example above the variable `FOOBAR` will end up representing the value `foo-bar` when expanded during execution of an action where it's referenced.

### Example 4: override variables

Each variable declared in the `pipeline` file can be overriden from the command line. This is convinient to inject context related information into the variables that you could not otherwise define in the file.

Continouing wiht the example from above where the variable `FOO=foo` is declared in the `pipeline` file, you can change the value to **bar** from the command line by executing the following variable override:

```shell
dotnet pipe FOO=bar
```

### Example 5: use environment variables

Just like variables you can reference environment variables inside the `pipeline` file. They can be referenced both inside variable declerations and also inside actions and they are expanded right before an action is executed - just like variables. Environment variables are referenced by using the `$` character and curly brackets like `${key}`. Here's how to include the value of an environment variable inside a regular `pipeline` file variable:

```
FOO=current user is ${USER}
```

The value of `FOO` will be fully expanded when an action is executed and will also include the value of the environment variable `USER`.

### Example 6: specify a shell to use underneath

`pipe` will execute all actions using a background shell and the shell is actually defined using a _hidden_ variable named `SHELL`. If the `SHELL` variable is not declared in the `pipeline` file the `pipe` runtime will automatically inject the variable during execution. The runtime will select a default shell based on the current operating system and this is how the selection works:

| OS      | Shell      |
| ------- | ---------- |
| Windows | powershell |
| Linux   | sh         |
| Mac     | bash       |

You can override the shell selection just by declaring the `SHELL` variable in your `pipeline` file. If you are running **Windows** and have **bash** installed, you can use bash as the background shell like this:

```
SHELL=bash
```

This will instruct `pipe` to use **bash** instead of **powershell** even though **powershell** would be the default choice on **Windows**. Currently you can only override the shell selection by using the shell names from the table above (e.g. `powershell`, `sh` and `bash`).

### Example 7: specify pipeline file location

By default `pipe` will look for a file named `pipeline` or `Pipeline` in the directory you are executing the `dotnet pipe` command. But you can specify any absolute path to a `pipeline` file by using the `-f [absolute path to file]` option:

```shell
dotnet pipe -f absolute-path-to-file
```

### Example 8: enable verbosity

You can use the `-v` options to turn on verbosity and get an insight into the context each action is executed in. This is usefull for inspection and debugging.

```shell
dotnet pipe -v
```

This will print a lot of debugging information to your console along side the output from the actions that you execute.

### Example 9: writing comments it your pipeline file

You can write comments in `pipeline` files by using the `#` character followed by your comment. You can start a line with the `#` character which means the whole line will be recognized as a comment and thus ignored during execution. You can also use the `#` character at any given location on a line and turn the rest of that line into a comment. Let's see that in action:

```
# this is a whole line comment
FOO=foo
BAR=bar # the rest of this line is also a comment
```

## Normal development flow

The goal of `pipe` is to require as little as possible to be installed on the environemnt that it is going to be used in. Another goal of `pipe` is that because it gives both structure and execution control of a pipeline, you should be able to specify most if not all the steps that your would require of a pipeline. Because `pipe` is built as a dotnet cli tool, you will need to install the tool before you can use it (kinda obvious).

This means that normally when joining a project that uses `pipe` you will have to execute the following to get all set up:

```shell
dotnet tool restore
```

This will download the currently pinned version of `pipe` that is used on that particular project. Then you can go ahead and use `pipe` as described above. The convention is to have the first step of your pipeline file to be a get everything set up on the environment and to run any initialization steps. I usually go with:

```
init: clean restore build test pack

clean:
    ...

restore:
    ...

build:
    ...

test:
    ...

pack:
    ...
```

This means that you make sure that a new development environment successfully can execute these steps before continuing with the development. This will also form the foundation of the continous integration part of a pipeline, making sure that if these steps can be successfully executed any changes can be integrated into the mainline. So with the above structure you can go ahead and execute:

```shell
dotnet pipe
```

As mentioned further up, this will default to executing the `init` step and in turn execute all the essential steps of your pipeline.

## Versioning scheme

`pipe` is versioned using [SemVer 2.0](https://semver.org/spec/v2.0.0.html)

## Contributions

Engage through GitHub - bugs, suggestions and PR's are welcome.

## License

The `pipe` project uses the **MIT** license.
