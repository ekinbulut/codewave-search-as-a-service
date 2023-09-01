// See https://aka.ms/new-console-template for more information
// Indexer: will consume publisher generated data and will index data to elasticache

using Indexer;

var app = new Application(args);
app.DisplayInfo();
app.RegisterConfigs();
app.RegisterServices();
await app.RunAsync();