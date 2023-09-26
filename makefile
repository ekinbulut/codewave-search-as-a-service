docker:
	docker compose up -d

build:
	dotnet build

run: docker
	dotnet run --project Indexer/Indexer.csproj
	dotnet run --project Publisher/Publisher.csproj
	
all: run