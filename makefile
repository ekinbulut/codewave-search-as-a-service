docker:
	docker compose up -d

build:
	dotnet build

run: docker
	dotnet run --project Indexer/Indexer.csproj

run-indexer:
	dotnet run -c Debug --project Indexer/Indexer.csproj
	