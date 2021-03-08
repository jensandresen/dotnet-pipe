CONFIGURATION=debug
OUTPUT_DIR=${PWD}/output

init: restore build test

restore:
	cd src && dotnet restore

build:
	cd src && dotnet build -c $(CONFIGURATION)

test:
	cd src && dotnet test --no-build --no-restore -c $(CONFIGURATION)

pack:
	rm -Rf $(OUTPUT_DIR)
	mkdir $(OUTPUT_DIR)
	cd src && dotnet pack --no-build --no-restore -c $(CONFIGURATION) -o $(OUTPUT_DIR)