CONFIGURATION=debug
OUTPUT_DIR=${PWD}/output

init: restore build test

clean:
	rm -Rf $(OUTPUT_DIR)
	mkdir $(OUTPUT_DIR)

restore:
	cd src && dotnet restore -v q

build:
	cd src && dotnet build -c $(CONFIGURATION) -v q

test:
	cd src && dotnet test --no-build --no-restore -c $(CONFIGURATION) -v q

pack:
	rm -Rf $(OUTPUT_DIR)
	mkdir $(OUTPUT_DIR)
	cd src && dotnet pack \
		--no-build \
		--no-restore \
		-c $(CONFIGURATION) \
		-o $(OUTPUT_DIR) \
		./pipe/pipe.csproj

reinstall:
	-dotnet tool uninstall pipe
	-rm -Rf ~/.nuget/packages/pipe
	dotnet tool install \
		--local \
		--no-cache \
		--add-source "$(OUTPUT_DIR)" \
		pipe