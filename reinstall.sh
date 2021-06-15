#!/bin/bash
make clean restore build test pack

# rm -Rf ./.config
# dotnet new tool-manifest
dotnet tool uninstall --local pipe
dotnet tool install --local --no-cache --add-source ./output pipe