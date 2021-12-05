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

release: CONFIGURATION=Release
release: clean restore build test pack

reinstall:
	-dotnet tool uninstall pipe
	-dotnet tool uninstall dotnet-pipeline
	-dotnet tool uninstall pipe-cli
	-rm -Rf ~/.nuget/packages/pipe
	-rm -Rf ~/.nuget/packages/dotnet-pipeline
	-rm -Rf ~/.nuget/packages/pipe-cli
	dotnet tool install \
		--local \
		--no-cache \
		--add-source "$(OUTPUT_DIR)" \
		pipe-cli

.PHONY: fulltest
fulltest: clean restore build test pack reinstall
	dotnet pipe build test -v -f Makefile

.PHONY: changelog
changelog:
	@echo "Generating changelog..."
	@docker run \
		-i \
		-v "${PWD}":/app \
		-e "WORKDIR=/app" \
		-e "CONFIG=cliff.toml" \
		-e "REPOSITRY=.git" \
		-e "OUTPUT=CHANGELOG.md" \
		orhunp/git-cliff:latest