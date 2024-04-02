help:
	@fgrep -h "##" $(MAKEFILE_LIST) | sed -e 's/##//' | tail -n +2

debug: ## Build debug
	dotnet build TinyWebSocket/*.csproj --configuration Debug

release: ## Build release
	dotnet build TinyWebSocket/*.csproj --configuration Release

test: ## Run tests
	./bin/test.sh
	dotnet test TinyWebSocket.Tests/*.csproj

copy: ## Copy files from project to UnityPackage
	rsync -avm --exclude='obj/' --exclude='bin/' --include='*.cs' --include='*/' --exclude='*' ./TinyWebSocket/ ./UnityPackage/
	cp LICENSE ./UnityPackage/
	cp README.md ./UnityPackage/

rcopy: ## Copy files from UnityPackage back to project
	rsync -avm --exclude='Editor/' --include='*.cs' --include='*/' --exclude='*' ./UnityPackage/ ./TinyWebSocket

clean: ## Clean project
	git clean -xdf
